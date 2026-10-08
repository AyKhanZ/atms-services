using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Models.WorkItems;
using System.Text.RegularExpressions;
using ATMS.Application.Interfaces;
using ATMS.Application.Models;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Enums;
using ATMS.Project.Data.Models.Comments;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Services.Domain.Comments.Interfaces;
using ATMS.Project.Services.Domain.Dictionaries.Interfaces;
using ATMS.Project.Services.Domain.Security.Interfaces;

namespace ATMS.Project.Services.Domain.Comments;

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

        // a deleted comment is a placeholder, its text, mentions and links are not read
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

    // only work from projects the reader can open, otherwise the title leaks
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

        var statuses = new Dictionary<CommentReferenceKindEnum, Dictionary<int, DictionaryModel>>();
        foreach (var kind in rows.Select(row => row.Kind).Distinct())
        {
            var list = kind switch
            {
                CommentReferenceKindEnum.Project => await dictionaries.GetProjectStatusesAsync(cancellationToken),
                CommentReferenceKindEnum.Ticket => await dictionaries.GetWorkTicketStatusesAsync(cancellationToken),
                _ => await dictionaries.GetWorkTaskStatusesAsync(cancellationToken)
            };
            statuses[kind] = list.ToDictionary(status => status.Id);
        }

        // a row we can't show stays plain text, one odd row shouldn't break the list
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
                    CommentReferenceKindEnum.Project => "project",
                    CommentReferenceKindEnum.Ticket => "ticket",
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

    // deleted user: the comment is still readable, just without a name
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
