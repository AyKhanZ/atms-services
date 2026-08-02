using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ATMS.Admin.Service.Infrastructure.Delivery;

public class DeliveryRetentionBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<DeliveryRetentionBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromDays(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CleanupInterval);

        do
        {
            try
            {
                await DeleteExpiredRecordsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to delete expired delivery records");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    protected virtual async Task DeleteExpiredRecordsAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var processedBefore = DateTime.UtcNow.AddDays(-30);
        var inboxProcessedBefore = DateTime.UtcNow.AddDays(-60);

        await scope.ServiceProvider
            .GetRequiredService<IOutboxRepository>()
            .DeleteProcessedBeforeAsync(processedBefore, cancellationToken);
        await scope.ServiceProvider
            .GetRequiredService<IEmailDeliveryRepository>()
            .DeleteProcessedBeforeAsync(processedBefore, cancellationToken);
        await scope.ServiceProvider
            .GetRequiredService<IInboxRepository>()
            .DeleteProcessedBeforeAsync(inboxProcessedBefore, cancellationToken);
    }
}
