using ATMS.Data.Messaging;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ATMS.Project.Services.Infrastructure;

public sealed class MessageRetentionBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<MessageRetentionBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromDays(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CleanupInterval);

        do
        {
            // each table separately, one failure shouldn't stop the other
            await DeleteAsync(
                "outbox",
                provider => provider.GetRequiredService<IOutboxRepository>()
                    .DeleteProcessedBeforeAsync(DateTime.UtcNow.AddDays(-30), stoppingToken),
                stoppingToken);
            await DeleteAsync(
                "inbox",
                provider => provider.GetRequiredService<IInboxRepository>()
                    .DeleteProcessedBeforeAsync(DateTime.UtcNow.AddDays(-60), stoppingToken),
                stoppingToken);
        }
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task DeleteAsync(
        string table,
        Func<IServiceProvider, Task> delete,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            await delete(scope.ServiceProvider);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to delete expired {Table} records", table);
        }
    }
}
