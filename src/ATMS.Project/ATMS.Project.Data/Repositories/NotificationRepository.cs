using ATMS.Data.Constants;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Interceptors;
using ATMS.Project.Data.Models.History;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public sealed class NotificationRepository(
    ProjectDbContext context,
    NotificationLockInterceptor locks) : INotificationRepository
{
    public Task LockForMergeAsync(IReadOnlyCollection<Guid> entityIds, CancellationToken cancellationToken) =>
        locks.LockAsync(context, entityIds, cancellationToken);

    public async Task<KeysetPagedResult<NotificationRow>> GetManyAsync(
        Guid userId,
        ACriteria<Notification> criteria,
        KeysetPaginationCriteria<NotificationRow> pagination,
        CancellationToken cancellationToken)
    {
        var rows = ToRows(criteria.Apply(OfUser(context.Notifications.AsNoTracking().IgnoreQueryFilters(), userId)));
        var items = await pagination
            .Apply(rows, row => row.CreatedAt, row => row.Id)
            .ToArrayAsync(cancellationToken);

        return pagination.ToResult(items, row => row.CreatedAt, row => row.Id);
    }

    public Task<NotificationRow?> GetRowAsync(Guid notificationId, CancellationToken cancellationToken) =>
        ToRows(context.Notifications.AsNoTracking().IgnoreQueryFilters()
                .Where(notification => notification.Id == notificationId))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<int> CountUnreadAsync(Guid userId, CancellationToken cancellationToken) =>
        OfUser(context.Notifications.AsNoTracking(), userId)
            .CountAsync(notification => notification.ReadAt == null, cancellationToken);

    public Task<Notification?> FindAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken) =>
        OfUser(context.Notifications, userId)
            .FirstOrDefaultAsync(notification => notification.Id == notificationId, cancellationToken);

    public Task<bool> IsExistAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken) =>
        OfUser(context.Notifications.AsNoTracking(), userId)
            .AnyAsync(notification => notification.Id == notificationId, cancellationToken);

    public Task<int> MarkAllReadAsync(Guid userId, DateTime readAt, CancellationToken cancellationToken) =>
        OfUser(context.Notifications, userId)
            .Where(notification => notification.ReadAt == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(notification => notification.ReadAt, readAt),
                cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);

    public Task<Notification[]> GetUnreadSinceAsync(
        IReadOnlyCollection<Guid> userIds,
        IReadOnlyCollection<Guid> entityIds,
        DateTime since,
        CancellationToken cancellationToken) =>
        context.Notifications
            .Where(notification =>
                userIds.Contains(notification.UserId) &&
                entityIds.Contains(notification.EntityId) &&
                notification.ReadAt == null &&
                notification.CreatedAt > since)
            .ToArrayAsync(cancellationToken);

    public Task<NotificationKeyRow[]> GetDedupKeysAsync(
        IReadOnlyCollection<Guid> userIds,
        IReadOnlyCollection<string> dedupKeys,
        CancellationToken cancellationToken) =>
        context.Notifications
            .AsNoTracking()
            .Where(notification =>
                userIds.Contains(notification.UserId) &&
                notification.DedupKey != null &&
                dedupKeys.Contains(notification.DedupKey))
            .Select(notification => new NotificationKeyRow(notification.UserId, notification.DedupKey!))
            .ToArrayAsync(cancellationToken);

    public Task<WorkTaskAudienceRow?> GetWorkTaskAudienceAsync(
        Guid projectId,
        Guid? assigneeId,
        CancellationToken cancellationToken) =>
        context.WorkProjects
            .AsNoTracking()
            .Where(project => project.Id == projectId)
            .Select(project => new WorkTaskAudienceRow(
                project.Title,
                project.WorkProjectParticipants
                    .Where(participant => participant.Id == assigneeId)
                    .Select(participant => (Guid?)participant.UserId)
                    .FirstOrDefault()))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<CommentedTaskRow?> GetCommentedTaskAsync(
        Guid projectId,
        Guid workTaskId,
        CancellationToken cancellationToken) =>
        context.WorkTasks
            .AsNoTracking()
            .Where(task => task.WorkProjectId == projectId && task.Id == workTaskId)
            .Select(task => new CommentedTaskRow(
                task.WorkProject.Title,
                task.Code,
                task.Title,
                task.ParentWorkTaskId != null,
                task.Assignee == null ? null : task.Assignee.UserId,
                task.CreatedById,
                context.Comments
                    .Where(comment => comment.OwnerType == (int)CommentOwnerTypeEnum.Task && comment.OwnerId == task.Id)
                    .Select(comment => comment.CreatedById)
                    .Distinct()
                    .ToArray()))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<DeadlineTaskRow[]> GetOpenTasksDueAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken) =>
        context.WorkTasks
            .AsNoTracking()
            .Where(task =>
                task.Deadline >= fromUtc &&
                task.Deadline < toUtc &&
                task.StatusId != (int)WorkTaskStatusEnum.Done &&
                !task.WorkProject.IsDeleted)
            .Select(task => new DeadlineTaskRow(
                task.Id,
                task.WorkProjectId,
                task.WorkProject.Title,
                task.Code,
                task.Title,
                task.ParentWorkTaskId != null,
                (DateTime)task.Deadline,
                task.Assignee == null ? null : task.Assignee.UserId,
                task.WorkProject.WorkProjectParticipants
                    .Where(participant => participant.WorkProjectParticipantRoles
                        .Any(role => role.RoleId == RoleIds.ProjectManager))
                    .Select(participant => participant.UserId)
                    .ToArray()))
            .ToArrayAsync(cancellationToken);

    // not project history, so a plain DELETE is ok; in batches so the table is never locked whole
    public async Task<int> DeleteOldAsync(
        DateTime readBefore,
        DateTime createdBefore,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var total = 0;
        while (true)
        {
            var deleted = await context.Notifications
                .Where(notification =>
                    notification.CreatedAt < createdBefore ||
                    (notification.ReadAt != null && notification.CreatedAt < readBefore))
                .OrderBy(notification => notification.CreatedAt)
                .ThenBy(notification => notification.Id)
                .Take(batchSize)
                .ExecuteDeleteAsync(cancellationToken);
            total += deleted;

            if (deleted < batchSize)
            {
                return total;
            }
        }
    }

    public Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken) =>
        context.Notifications.AddRangeAsync(notifications, cancellationToken);

    // the caller lifts query filters: a deleted actor keeps the name, deleted task/project/comment is checked here
    private IQueryable<NotificationRow> ToRows(IQueryable<Notification> notifications) =>
        from notification in notifications
        // the task is read live: it can move to another ticket, get done or get a new deadline
        join task in context.WorkTasks on notification.EntityId equals task.Id into tasks
        from task in tasks.DefaultIfEmpty()
        select new NotificationRow
        {
            Id = notification.Id,
            Type = notification.Type,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt,
            Actor = notification.Actor == null
                ? null
                : new HistoryPerson(
                    notification.Actor.Id,
                    notification.Actor.Name,
                    notification.Actor.Surname,
                    notification.Actor.AvatarPath),
            WorkProjectId = notification.WorkProjectId,
            EntityType = notification.EntityType,
            EntityId = notification.EntityId,
            WorkTicketId = task != null && !task.IsDeleted ? task.WorkTicketId : null,
            TaskStatusId = task != null && !task.IsDeleted ? task.StatusId : null,
            TaskDeadline = task != null && !task.IsDeleted ? task.Deadline : null,
            CommentId = notification.CommentId,
            Parameters = notification.Parameters,
            EntityDeleted = notification.EntityType == (int)NotificationEntityTypeEnum.WorkTask
                ? task == null || task.IsDeleted || task.WorkProject.IsDeleted
                : !context.WorkProjects.Any(project =>
                    project.Id == notification.EntityId &&
                    !project.IsDeleted),
            CommentDeleted = notification.CommentId != null &&
                             !context.Comments.Any(comment =>
                                 comment.Id == notification.CommentId &&
                                 !comment.IsDeleted)
        };

    private static IQueryable<Notification> OfUser(IQueryable<Notification> query, Guid userId) =>
        query.Where(notification => notification.UserId == userId);
}
