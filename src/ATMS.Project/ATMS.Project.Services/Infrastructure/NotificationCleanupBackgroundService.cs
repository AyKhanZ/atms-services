using Microsoft.Extensions.Options;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ATMS.Project.Services.Infrastructure;

// read ones go after 30 days, any after 90 (~135k rows for 50 people), emails go with them
public sealed class NotificationCleanupBackgroundService(
    IServiceScopeFactory scopeFactory,
    BusinessTimeZone businessTimeZone,
    IOptions<NotificationsOptions> notificationsOptions,
    ILogger<NotificationCleanupBackgroundService> logger) : BackgroundService
{
    private const int BatchSize = 1000;

    private readonly NotificationsOptions _options = notificationsOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DeleteOldAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var next = businessTimeZone.NextUtc(DateTime.UtcNow, _options.CleanupTime);
            try
            {
                await Task.Delay(next - DateTime.UtcNow, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await DeleteOldAsync(stoppingToken);
        }
    }

    private async Task DeleteOldAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var now = DateTime.UtcNow;
            await scope.ServiceProvider
                .GetRequiredService<INotificationRepository>()
                .DeleteOldAsync(
                    now.AddDays(-_options.ReadRetentionDays),
                    now.AddDays(-_options.RetentionDays),
                    BatchSize,
                    cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Notification cleanup failed; the next pass is tomorrow");
        }
    }
}
