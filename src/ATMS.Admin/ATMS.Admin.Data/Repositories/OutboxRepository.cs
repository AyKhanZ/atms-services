using System.Text.Json;
using ATMS.Admin.Data.DbContexts;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Data.Enums;
using ATMS.Data.Messaging;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Admin.Data.Repositories;

public class OutboxRepository(AdminDbContext context) : IOutboxRepository
{
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
            Status = DeliveryStatusEnum.Pending,
            CreatedAt = now,
            NextAttemptAt = now
        };

        await context.OutboxMessages.AddAsync(entity, cancellationToken);
        return entity.Id;
    }

    public async Task<List<OutboxMessage>> ClaimPendingAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return await context.OutboxMessages
            .AsNoTracking()
            .Where(x => x.Status == DeliveryStatusEnum.Pending && x.NextAttemptAt <= now)
            .OrderBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkProcessedAsync(Guid id, CancellationToken cancellationToken)
    {
        var message = await context.OutboxMessages
            .FirstAsync(x => x.Id == id, cancellationToken);

        message.Status = DeliveryStatusEnum.Processed;
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
        var message = await context.OutboxMessages
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
        var message = await context.OutboxMessages
            .FirstAsync(x => x.Id == id, cancellationToken);

        message.Status = DeliveryStatusEnum.Failed;
        message.AttemptCount = attemptCount;
        message.FailedAt = DateTime.UtcNow;
        message.LastError = error.Length > 2000 ? error[..2000] : error;

        await context.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteProcessedBeforeAsync(
        DateTime processedBefore,
        CancellationToken cancellationToken)
    {
        return context.OutboxMessages
            .Where(x => x.Status == DeliveryStatusEnum.Processed &&
                        x.ProcessedAt < processedBefore)
            .ExecuteDeleteAsync(cancellationToken);
    }

}
