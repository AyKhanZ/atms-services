using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Models.WorkItems;
using System.Text.RegularExpressions;
using ATMS.Application.Interfaces;
using ATMS.Application.Models;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Comments;
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
    ICurrentUser currentUser,
    ICommentMentionService mentions) : ICommentModelService
{
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
        var userIds = items
            .Select(item => item.CreatedById)
            .Concat(items.Select(item => item.DeletedById).OfType<Guid>())
            .Distinct()
            .ToArray();
        var authors = (await comments.GetAuthorsAsync(userIds, cancellationToken))
            .ToDictionary(user => user.Id);

        // A deleted comment is a placeholder: its text, mentions and links are never read out.
        var mentionsByComment = items.ToDictionary(
            item => item.Id,
            item => item.IsDeleted ? [] : mentions.GetMentionedUserIds(item.Text));
        var mentionIds = mentionsByComment.Values.SelectMany(ids => ids).Distinct().ToArray();
        var mentioned = mentionIds.Length == 0
            ? new Dictionary<Guid, User>()
            : (await comments.GetMentionedParticipantsAsync(projectId, mentionIds, cancellationToken))
                .DistinctBy(user => user.Id)
                .ToDictionary(user => user.Id);

        var referenceCodesByComment = items.ToDictionary(
            item => item.Id,
            item => item.IsDeleted ? [] : ParseReferenceCodes(item.Text));
        var codes = referenceCodesByComment.Values.SelectMany(values => values).Distinct().ToArray();
        var references = await GetReferencesAsync(codes, cancellationToken);

        return items.ToDictionary(item => item.Id, item =>
        {
            var isOwn = item.CreatedById == currentUser.Id;
            return new CommentModel
            {
                Id = item.Id,
                Text = item.IsDeleted ? string.Empty : item.Text,
                CreatedAt = item.CreatedAt,
                CreatedBy = PersonOf(authors, item.CreatedById),
                UpdatedAt = item.UpdatedAt,
                IsDeleted = item.IsDeleted,
                DeletedAt = item.IsDeleted ? item.DeletedAt : null,
                DeletedBy = item is { IsDeleted: true, DeletedById: { } deletedById }
                    ? PersonOf(authors, deletedById)
                    : null,
                CanEdit = !item.IsDeleted && isOwn && canWrite,
                CanDelete = !item.IsDeleted && (isOwn ? canWrite : canDeleteOthers),
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

        // Each kind has statuses of its own; only the kinds found are read.
        var statuses = new Dictionary<CommentReferenceKind, Dictionary<int, DictionaryModel>>();
        foreach (var kind in rows.Select(row => row.Kind).Distinct())
        {
            var list = kind switch
            {
                CommentReferenceKind.Project => await dictionaries.GetProjectStatusesAsync(cancellationToken),
                CommentReferenceKind.Ticket => await dictionaries.GetWorkTicketStatusesAsync(cancellationToken),
                _ => await dictionaries.GetWorkTaskStatusesAsync(cancellationToken)
            };
            statuses[kind] = list.ToDictionary(status => status.Id);
        }

        // A row that cannot be shown — a code met twice, a status the cache does not know yet — is
        // left as plain text, so one odd row never takes the whole list down.
        var references = new Dictionary<string, CommentReferenceModel>();
        foreach (var row in rows)
        {
            if (references.ContainsKey(row.Code) || !statuses[row.Kind].TryGetValue(row.StatusId, out var status))
            {
                continue;
            }

            references[row.Code] = new CommentReferenceModel
            {
                Code = row.Code,
                Type = row.Kind switch
                {
                    CommentReferenceKind.Project => "project",
                    CommentReferenceKind.Ticket => "ticket",
                    _ => "task"
                },
                IsSubtask = row.IsSubtask,
                Title = row.Title,
                Status = status,
                Ref = new WorkItemRefModel
                {
                    ProjectId = row.ProjectId,
                    WorkTicketId = row.WorkTicketId,
                    WorkTaskId = row.WorkTaskId
                }
            };
        }

        return references;
    }

    // A user gone from the users table still leaves the comment readable, unnamed.
    private static PersonModel PersonOf(Dictionary<Guid, User> users, Guid id) =>
        users.TryGetValue(id, out var user)
            ? ToPerson(user)
            : new PersonModel { Id = id, Name = string.Empty, Surname = string.Empty };

    private static PersonModel ToPerson(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Surname = user.Surname,
        AvatarPath = user.AvatarPath
    };
}
