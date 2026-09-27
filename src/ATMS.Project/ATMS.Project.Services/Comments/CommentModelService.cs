using System.Text.RegularExpressions;
using ATMS.Application.Interfaces;
using ATMS.Application.Models;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Contracts.Models.Dashboard;
using ATMS.Project.Contracts.Models.History;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Comments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Services.Comments.Interfaces;
using ATMS.Project.Services.Dictionaries.Interfaces;
using ATMS.Project.Services.Security.Interfaces;

namespace ATMS.Project.Services.Comments;

public sealed class CommentModelService(
    ICommentRepository comments,
    IProjectPermissionService permissions,
    IDictionaryCacheService dictionaries,
    ICurrentUser currentUser) : ICommentModelService
{
    private static readonly Regex MentionPattern = new(
        @"@\[user:([0-9a-fA-F-]{36})\]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    private static readonly Regex ReferencePattern = new(
        @"`[^`]*`|\[[^\]]*\]\([^)]*\)|(?<![\p{L}\p{N}_#])#(?<code>[0-9]+)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    public async Task<IReadOnlyDictionary<Guid, CommentModel>> BuildAsync(
        Guid projectId,
        IReadOnlyCollection<Comment> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return new Dictionary<Guid, CommentModel>();
        }

        var permissionCodes = await permissions.GetPermissionCodesAsync(projectId, cancellationToken);
        var canWrite = permissions.IsSuperAdmin || permissionCodes.Contains(nameof(ProjectPermissionEnum.CommentEdit));
        var canDeleteOthers = permissions.IsSuperAdmin || permissionCodes.Contains(nameof(ProjectPermissionEnum.CommentDelete));
        var authors = (await comments.GetAuthorsAsync(
            items.Select(item => item.CreatedById).Distinct().ToArray(), cancellationToken))
            .ToDictionary(user => user.Id);

        var mentionsByComment = items.ToDictionary(
            item => item.Id,
            item => ParseMentions(item.Text));
        var mentionIds = mentionsByComment.Values.SelectMany(ids => ids).Distinct().ToArray();
        var mentioned = mentionIds.Length == 0
            ? new Dictionary<Guid, User>()
            : (await comments.GetMentionedParticipantsAsync(projectId, mentionIds, cancellationToken))
                .DistinctBy(user => user.Id)
                .ToDictionary(user => user.Id);

        var referenceCodesByComment = items.ToDictionary(
            item => item.Id,
            item => ParseReferenceCodes(item.Text));
        var codes = referenceCodesByComment.Values.SelectMany(values => values).Distinct().ToArray();
        var references = await GetReferencesAsync(codes, cancellationToken);

        return items.ToDictionary(item => item.Id, item =>
        {
            var isOwn = item.CreatedById == currentUser.Id;
            return new CommentModel
            {
                Id = item.Id,
                Text = item.Text,
                CreatedAt = item.CreatedAt,
                CreatedBy = ToPerson(authors[item.CreatedById]),
                UpdatedAt = item.UpdatedAt,
                CanEdit = isOwn && canWrite,
                CanDelete = isOwn ? canWrite : canDeleteOthers,
                Mentions = mentionsByComment[item.Id]
                    .Where(mentioned.ContainsKey)
                    .Select(id => ToPerson(mentioned[id]))
                    .ToArray(),
                References = referenceCodesByComment[item.Id]
                    .Where(references.ContainsKey)
                    .Select(code => references[code])
                    .ToArray()
            };
        });
    }

    private static Guid[] ParseMentions(string text)
    {
        return MentionPattern.Matches(text)
            .Select(match => Guid.TryParse(match.Groups[1].Value, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();
    }

    private static string[] ParseReferenceCodes(string text) =>
        ReferencePattern.Matches(text)
            .Where(match => match.Groups["code"].Success)
            .Select(match => match.Groups["code"].Value)
            .Distinct()
            .ToArray();

    // Only work in projects the reader can open: a code from someone else's project stays plain
    // text, so its title does not leak. Status names come from the cached dictionaries, translated.
    private async Task<Dictionary<string, CommentReferenceModel>> GetReferencesAsync(
        string[] codes,
        CancellationToken cancellationToken)
    {
        if (codes.Length == 0)
        {
            return [];
        }

        var rows = await comments.GetReferencesAsync(
            codes,
            new AccessibleWorkProjectsCriteria(currentUser.Id, currentUser.RoleId),
            cancellationToken);
        if (rows.Length == 0)
        {
            return [];
        }

        var taskStatuses = rows.Any(row => !row.IsTicket)
            ? (await dictionaries.GetWorkTaskStatusesAsync(cancellationToken)).ToDictionary(status => status.Id)
            : [];
        var ticketStatuses = rows.Any(row => row.IsTicket)
            ? (await dictionaries.GetWorkTicketStatusesAsync(cancellationToken)).ToDictionary(status => status.Id)
            : [];

        return rows.ToDictionary(row => row.Code, row => new CommentReferenceModel
        {
            Code = row.Code,
            Type = row.IsTicket ? "ticket" : "task",
            IsSubtask = row.IsSubtask,
            Title = row.Title,
            Status = (row.IsTicket ? ticketStatuses : taskStatuses)[row.StatusId],
            Ref = new DashboardRefModel
            {
                ProjectId = row.ProjectId,
                WorkTicketId = row.WorkTicketId,
                WorkTaskId = row.WorkTaskId
            }
        });
    }

    private static HistoryPersonModel ToPerson(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Surname = user.Surname,
        AvatarPath = user.AvatarPath
    };
}
