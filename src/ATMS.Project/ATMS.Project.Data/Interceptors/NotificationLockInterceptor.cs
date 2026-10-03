using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace ATMS.Project.Data.Interceptors;

// A merge into an unread notification reads, then writes. Two changes of one task at the same moment
// would both read "nothing unread" and both write a new row. A lock per task, held until the commit,
// makes the second change wait for the first and then merge into what the first wrote. Changes of
// other tasks never wait for each other.
//
// The lock needs a transaction that ends with the change itself. When the caller has none, one is
// opened here and committed right after the next SaveChanges, so handlers stay as they are. A change
// that never reaches SaveChanges leaves the transaction to be rolled back with the context.
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

        // Always in the same order, so two changes that lock the same tasks cannot wait for each other.
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
