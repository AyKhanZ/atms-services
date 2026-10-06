using ATMS.Data.Messaging;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ATMS.Project.Services.Infrastructure;

public class MessageRetentionBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<MessageRetentionBackgroundService> logger) : BackgroundService
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
                logger.LogError(exception, "Failed to delete expired inbox and outbox records");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task DeleteExpiredRecordsAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        await scope.ServiceProvider
            .GetRequiredService<IOutboxRepository>()
            .DeleteProcessedBeforeAsync(DateTime.UtcNow.AddDays(-30), cancellationToken);
        await scope.ServiceProvider
            .GetRequiredService<IInboxRepository>()
            .DeleteProcessedBeforeAsync(DateTime.UtcNow.AddDays(-60), cancellationToken);
    }
}
