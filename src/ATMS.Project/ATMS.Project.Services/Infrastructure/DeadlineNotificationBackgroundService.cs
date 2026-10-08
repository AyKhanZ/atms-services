using Microsoft.Extensions.Options;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;
using ATMS.Project.Services.Domain.Notifications.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ATMS.Project.Services.Infrastructure;

// once a day at a fixed business hour (not every 24h from start), so people get it in the morning
public sealed class DeadlineNotificationBackgroundService(
    IServiceScopeFactory scopeFactory,
    BusinessTimeZone businessTimeZone,
    IOptions<NotificationsOptions> notificationsOptions,
    ILogger<DeadlineNotificationBackgroundService> logger) : BackgroundService
{
    private readonly NotificationsOptions _options = notificationsOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // started after the pass time: run now, the dedup keys stop double sends
        if (businessTimeZone.HasReached(DateTime.UtcNow, _options.DeadlineReminderTime))
        {
            await RemindAsync(stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var next = businessTimeZone.NextUtc(DateTime.UtcNow, _options.DeadlineReminderTime);
            try
            {
                await Task.Delay(next - DateTime.UtcNow, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await RemindAsync(stoppingToken);
        }
    }

    private async Task RemindAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            await scope.ServiceProvider
                .GetRequiredService<IDeadlineNotificationService>()
                .RemindAsync(DateTime.UtcNow, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Deadline reminders failed; the next pass is tomorrow");
        }
    }
}
