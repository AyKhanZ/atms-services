using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;
using ATMS.Project.Services.Notifications.Interfaces;
using ATMS.Project.Services.Time;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ATMS.Project.Services.Infrastructure;

// Deadline reminders once a day at a fixed hour of the business day, not every 24 hours from start:
// people get them with their morning, whenever the service was restarted.
public sealed class DeadlineNotificationBackgroundService(
    IServiceScopeFactory scopeFactory,
    BusinessTimeZone businessTimeZone,
    IConfiguration configuration,
    ILogger<DeadlineNotificationBackgroundService> logger) : BackgroundService
{
    private readonly NotificationsOptions _options =
        configuration.GetSection(nameof(NotificationsOptions)).Get<NotificationsOptions>()
        ?? throw new ConfigurationException(
            ConfigurationErrorType.NotificationsSectionNotFound,
            string.Format(LogMessages.ConfigSectionNotFound, nameof(NotificationsOptions)));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Down at the hour of the pass: it runs on start instead. The reminder keys keep the
        // morning's pass, if there was one, from being sent twice.
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
