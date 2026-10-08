using System.Text.Json;
using ATMS.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Data.Messaging;

// admin and project have their own outbox table, the context decides which
public sealed class OutboxRepository<TContext>(TContext context) : IOutboxRepository
    where TContext : DbContext
{
    private DbSet<OutboxMessage> OutboxMessages => context.Set<OutboxMessage>();

    public Task<bool> ContainsAsync<T>(
        string exchange,
        string routingKey,
        T message,
        CancellationToken cancellationToken)
    {
        var messageType = typeof(T).FullName ?? typeof(T).Name;
        var payload = JsonSerializer.Serialize(message);

        return OutboxMessages.AnyAsync(
            x => x.Exchange == exchange &&
                 x.RoutingKey == routingKey &&
                 x.MessageType == messageType &&
                 x.Payload == payload,
            cancellationToken);
    }

    public async Task<Guid> AddAsync<T>(
        string exchange,
        string routingKey,
        T message,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var entity = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Exchange = exchange,
            RoutingKey = routingKey,
            MessageType = typeof(T).FullName ?? typeof(T).Name,
            Payload = JsonSerializer.Serialize(message),
            Status = (int)DeliveryStatusEnum.Pending,
            CreatedAt = now,
            NextAttemptAt = now
        };

        await OutboxMessages.AddAsync(entity, cancellationToken);
        return entity.Id;
    }

    public async Task<List<OutboxMessage>> ClaimPendingAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return await OutboxMessages
            .AsNoTracking()
            .Where(x => x.Status == (int)DeliveryStatusEnum.Pending && x.NextAttemptAt <= now)
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken)
    {
        var message = await OutboxMessages
            .FirstAsync(x => x.Id == id, cancellationToken);

        message.Status = (int)DeliveryStatusEnum.Processed;
        message.ProcessedAt = DateTime.UtcNow;
        message.LastError = null;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkRetryAsync(
        Guid id,
        int attemptCount,
        DateTime nextAttemptAt,
        string error,
        CancellationToken cancellationToken)
    {
        var message = await OutboxMessages
            .FirstAsync(x => x.Id == id, cancellationToken);

        message.AttemptCount = attemptCount;
        message.NextAttemptAt = nextAttemptAt;
        message.LastError = error.Length > 2000 ? error[..2000] : error;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkFailedAsync(
        Guid id,
        int attemptCount,
        string error,
        CancellationToken cancellationToken)
    {
        var message = await OutboxMessages
            .FirstAsync(x => x.Id == id, cancellationToken);

        message.Status = (int)DeliveryStatusEnum.Failed;
        message.AttemptCount = attemptCount;
        message.FailedAt = DateTime.UtcNow;
        message.LastError = error.Length > 2000 ? error[..2000] : error;

        await context.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteProcessedBeforeAsync(
        DateTime processedBefore,
        CancellationToken cancellationToken)
    {
        return OutboxMessages
            .Where(x => x.Status == (int)DeliveryStatusEnum.Processed &&
                        x.ProcessedAt < processedBefore)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
