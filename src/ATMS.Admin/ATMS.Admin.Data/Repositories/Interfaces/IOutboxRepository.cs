using ATMS.Data.Messaging;

namespace ATMS.Admin.Data.Repositories.Interfaces;

public interface IOutboxRepository
{
    Task<Guid> AddAsync<T>(
        string exchange,
        string routingKey,
        T message,
        CancellationToken cancellationToken);

    Task<List<OutboxMessage>> ClaimPendingAsync(
        int batchSize,
        CancellationToken cancellationToken);

    Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken);

    Task MarkRetryAsync(
        Guid id,
        int attemptCount,
        DateTime nextAttemptAt,
        string error,
        CancellationToken cancellationToken);

    Task MarkFailedAsync(
        Guid id,
        int attemptCount,
        string error,
        CancellationToken cancellationToken);

    Task DeleteProcessedBeforeAsync(
        DateTime processedBefore,
        CancellationToken cancellationToken);
}
