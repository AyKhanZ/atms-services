using System.Data.Common;
using ATMS.Application.Realtime;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ATMS.Project.Data.Interceptors;

// The bell of every person who got something: notification.created for a new or merged
// notification, notification.read when only what they read changed. Pushed after the commit, the
// same way ProjectRealtimeInterceptor pushes changes of work: nothing for a change that was rolled
// back. One event per person per commit.
public sealed class NotificationRealtimeInterceptor(
    IRealtimeEventPublisher publisher,
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationRealtimeInterceptor> logger) : SaveChangesInterceptor, IDbTransactionInterceptor
{
    // By recipient: the newest notification they got, or null when only what they read changed.
    private readonly Dictionary<Guid, Guid?> _current = [];
    private readonly Dictionary<Guid, Guid?> _pending = [];

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Collect(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Collect(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        MergeCurrent();
        if (eventData.Context?.Database.CurrentTransaction is null)
        {
            PublishPendingAsync().GetAwaiter().GetResult();
        }

        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        MergeCurrent();
        if (eventData.Context?.Database.CurrentTransaction is null)
        {
            await PublishPendingAsync();
        }

        return result;
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData) => Clear();

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        Clear();
        return Task.CompletedTask;
    }

    public DbTransaction TransactionStarted(
        DbConnection connection,
        TransactionEndEventData eventData,
        DbTransaction result)
    {
        _pending.Clear();
        return result;
    }

    public ValueTask<DbTransaction> TransactionStartedAsync(
        DbConnection connection,
        TransactionEndEventData eventData,
        DbTransaction result,
        CancellationToken cancellationToken = default)
    {
        _pending.Clear();
        return ValueTask.FromResult(result);
    }

    public void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData) =>
        PublishPendingAsync().GetAwaiter().GetResult();

    public Task TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default) => PublishPendingAsync();

    public void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData) => Clear();

    public Task TransactionRolledBackAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        Clear();
        return Task.CompletedTask;
    }

    private void Collect(DbContext? dbContext)
    {
        _current.Clear();
        if (dbContext is not ProjectDbContext context)
        {
            return;
        }

        context.ChangeTracker.DetectChanges();
        foreach (var entry in context.ChangeTracker.Entries<Notification>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            Collect(entry);
        }
    }

    // A merge into an unread notification moves its time up: for the person it is news again.
    private void Collect(EntityEntry<Notification> entry)
    {
        var notification = entry.Entity;
        if (entry.State == EntityState.Added || entry.Property(nameof(Notification.CreatedAt)).IsModified)
        {
            _current[notification.UserId] = notification.Id;
        }
        else if (entry.Property(nameof(Notification.ReadAt)).IsModified)
        {
            _current.TryAdd(notification.UserId, null);
        }
    }

    // The newest notification of a person wins; a read after it keeps it.
    private void MergeCurrent()
    {
        foreach (var (userId, notificationId) in _current)
        {
            _pending[userId] = notificationId ?? _pending.GetValueOrDefault(userId);
        }

        _current.Clear();
    }

    private async Task PublishPendingAsync()
    {
        var notifications = _pending.ToArray();
        _pending.Clear();
        if (notifications.Length == 0)
        {
            return;
        }

        Dictionary<Guid, int> counts;
        try
        {
            counts = await CountUnreadAsync(notifications.Select(pair => pair.Key).ToArray());
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unread notification count failed; the bell is not pushed");
            return;
        }

        foreach (var (userId, notificationId) in notifications)
        {
            var unreadCount = counts.GetValueOrDefault(userId);
            try
            {
                if (notificationId is { } id)
                {
                    await publisher.PublishToUsersAsync(
                        [userId],
                        RealtimeEventNames.NotificationCreated,
                        new NotificationCreatedEvent(id, unreadCount),
                        CancellationToken.None);
                }
                else
                {
                    await publisher.PublishToUsersAsync(
                        [userId],
                        RealtimeEventNames.NotificationRead,
                        new NotificationReadEvent(unreadCount),
                        CancellationToken.None);
                }
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Realtime push failed for notifications of user {UserId}", userId);
            }
        }
    }

    // Counted after the commit, on a context of its own: a failing count must never touch the
    // transaction of the change itself, and the committed transaction cannot run a query anyway.
    private async Task<Dictionary<Guid, int>> CountUnreadAsync(IReadOnlyCollection<Guid> userIds)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();
        return await context.Notifications
            .AsNoTracking()
            .Where(notification => userIds.Contains(notification.UserId) && notification.ReadAt == null)
            .GroupBy(notification => notification.UserId)
            .Select(group => new { UserId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.UserId, row => row.Count);
    }

    private void Clear()
    {
        _current.Clear();
        _pending.Clear();
    }
}
