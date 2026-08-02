using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Data.Messaging;
using ATMS.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ATMS.Admin.Service.Infrastructure.Delivery;

public class OutboxBackgroundService(
    IServiceScopeFactory scopeFactory,
    DeliveryRetrySchedule retrySchedule,
    ILogger<OutboxBackgroundService> logger) : BackgroundService
{
    private const int BatchSize = 20;
    private static readonly TimeSpan EmptyQueueDelay = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processedCount = await ProcessBatchAsync(stoppingToken);
                if (processedCount == 0)
                {
                    await Task.Delay(EmptyQueueDelay, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Outbox worker failed while loading a delivery batch");
                await Task.Delay(EmptyQueueDelay, stoppingToken);
            }
        }
    }

    protected virtual async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        List<OutboxMessage> messages;
        await using (var scope = scopeFactory.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            messages = await repository.ClaimPendingAsync(BatchSize, cancellationToken);
        }

        foreach (var message in messages)
        {
            await ProcessMessageAsync(message, cancellationToken);
        }

        return messages.Count;
    }

    private async Task ProcessMessageAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var publisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();
            await publisher.PublishAsync(
                message.Exchange,
                message.RoutingKey,
                message.MessageType,
                message.Payload,
                message.Id,
                message.CreatedAt,
                cancellationToken);

            await repository.MarkProcessedAsync(message.Id, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            await HandleFailureAsync(message.Id, message.AttemptCount, exception, cancellationToken);
        }
    }

    private async Task HandleFailureAsync(
        Guid messageId,
        int previousAttemptCount,
        Exception exception,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var attemptCount = previousAttemptCount + 1;
        logger.LogError(
            exception,
            "Outbox message {MessageId} failed on attempt {AttemptCount}",
            messageId,
            attemptCount);

        if (attemptCount >= retrySchedule.MaxAttemptCount)
        {
            await repository.MarkFailedAsync(messageId, attemptCount, exception.Message, cancellationToken);
            return;
        }

        await repository.MarkRetryAsync(
            messageId,
            attemptCount,
            retrySchedule.GetNextAttemptAt(attemptCount),
            exception.Message,
            cancellationToken);
    }
}
