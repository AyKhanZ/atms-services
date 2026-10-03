using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Notifications;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<KeysetPagedResult<NotificationRow>> GetManyAsync(
        Guid userId,
        ACriteria<Notification> criteria,
        KeysetPaginationCriteria<NotificationRow> pagination,
        CancellationToken cancellationToken);

    Task<NotificationRow?> GetRowAsync(Guid notificationId, CancellationToken cancellationToken);

    Task<int> CountUnreadAsync(Guid userId, CancellationToken cancellationToken);

    Task<Notification?> FindAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken);

    Task<bool> IsExistAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken);

    Task<int> MarkAllReadAsync(Guid userId, DateTime readAt, CancellationToken cancellationToken);

    Task LockForMergeAsync(IReadOnlyCollection<Guid> entityIds, CancellationToken cancellationToken);

    Task<Notification[]> GetUnreadSinceAsync(
        IReadOnlyCollection<Guid> userIds,
        IReadOnlyCollection<Guid> entityIds,
        DateTime since,
        CancellationToken cancellationToken);

    Task<NotificationKeyRow[]> GetDedupKeysAsync(
        IReadOnlyCollection<Guid> userIds,
        IReadOnlyCollection<string> dedupKeys,
        CancellationToken cancellationToken);

    Task<WorkTaskAudienceRow?> GetWorkTaskAudienceAsync(
        Guid projectId,
        Guid? assigneeId,
        CancellationToken cancellationToken);

    Task<CommentedTaskRow?> GetCommentedTaskAsync(
        Guid projectId,
        Guid workTaskId,
        CancellationToken cancellationToken);

    Task<DeadlineTaskRow[]> GetOpenTasksDueAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken);

    Task<int> DeleteOldAsync(
        DateTime readBefore,
        DateTime createdBefore,
        int batchSize,
        CancellationToken cancellationToken);

    Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
