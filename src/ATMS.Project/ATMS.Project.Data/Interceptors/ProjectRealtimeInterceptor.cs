using System.Data.Common;
using ATMS.Application.Realtime;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ATMS.Project.Data.Interceptors;

public sealed class ProjectRealtimeInterceptor(
    IRealtimeEventPublisher publisher,
    ILogger<ProjectRealtimeInterceptor> logger) : SaveChangesInterceptor, IDbTransactionInterceptor
{
    private readonly Dictionary<(string EntityType, Guid Id), WorkItemChangedEvent> _currentSave = [];
    private readonly Dictionary<(string EntityType, Guid Id), WorkItemChangedEvent> _pending = [];

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
        Saved(eventData.Context?.Database.CurrentTransaction is not null);
        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await SavedAsync(eventData.Context?.Database.CurrentTransaction is not null);
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
        _currentSave.Clear();
        if (dbContext is not ProjectDbContext context)
        {
            return;
        }

        context.ChangeTracker.DetectChanges();
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
            {
                continue;
            }

            var (entityType, projectId, id) = entry.Entity switch
            {
                WorkTicket ticket => ("ticket", ticket.WorkProjectId, ticket.Id),
                WorkTask task => ("task", task.WorkProjectId, task.Id),
                _ => (string.Empty, Guid.Empty, Guid.Empty)
            };

            if (id == Guid.Empty || entityType.Length == 0)
            {
                continue;
            }

            var deleted = entry.Property(nameof(WorkTask.IsDeleted));
            var action = deleted.OriginalValue is false && deleted.CurrentValue is true
                ? "deleted"
                : entry.State == EntityState.Added ? "created" : "updated";

            _currentSave[(entityType, id)] = new WorkItemChangedEvent(projectId, entityType, id, action);
        }
    }

    private void Saved(bool inTransaction)
    {
        MergeCurrent();
        if (!inTransaction)
        {
            PublishPendingAsync().GetAwaiter().GetResult();
        }
    }

    internal async Task SavedAsync(bool inTransaction)
    {
        MergeCurrent();
        if (!inTransaction)
        {
            await PublishPendingAsync();
        }
    }

    private void MergeCurrent()
    {
        foreach (var (key, change) in _currentSave)
        {
            _pending[key] = _pending.TryGetValue(key, out var earlier) && earlier.Action == "created" && change.Action == "updated"
                ? earlier
                : change;
        }

        _currentSave.Clear();
    }

    private async Task PublishPendingAsync()
    {
        var changes = _pending.Values.ToArray();
        _pending.Clear();

        foreach (var change in changes)
        {
            try
            {
                await publisher.PublishToProjectAsync(
                    change.ProjectId,
                    RealtimeEventNames.WorkItemChanged,
                    change,
                    CancellationToken.None);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Realtime push failed for {EntityType} {EntityId}", change.EntityType, change.Id);
            }
        }
    }

    private void Clear()
    {
        _currentSave.Clear();
        _pending.Clear();
    }
}
