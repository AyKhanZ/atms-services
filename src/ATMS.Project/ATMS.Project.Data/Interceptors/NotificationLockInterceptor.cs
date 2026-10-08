using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace ATMS.Project.Data.Interceptors;

// two changes of one task at the same time would both see "nothing unread" and both insert a row.
// a lock per task until the commit makes the second one wait and merge into the first.
// no transaction? one is opened here and committed after the next SaveChanges
public sealed class NotificationLockInterceptor : SaveChangesInterceptor
{
    private IDbContextTransaction? _owned;

    public async Task LockAsync(
        DbContext context,
        IReadOnlyCollection<Guid> entityIds,
        CancellationToken cancellationToken)
    {
        if (context.Database.CurrentTransaction is null)
        {
            _owned = await context.Database.BeginTransactionAsync(cancellationToken);
        }

        // same order everywhere, so no deadlock
        foreach (var entityId in entityIds.Distinct().Order())
        {
            var key = $"notification:{entityId}";
            await context.Database.ExecuteSqlAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({key}, 0))",
                cancellationToken);
        }
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (_owned is { } transaction)
        {
            _owned = null;
            transaction.Commit();
            transaction.Dispose();
        }

        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (_owned is { } transaction)
        {
            _owned = null;
            await transaction.CommitAsync(cancellationToken);
            await transaction.DisposeAsync();
        }

        return result;
    }
}
