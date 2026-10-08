using ATMS.Admin.Data.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ATMS.Admin.Service.Infrastructure;

public sealed class ExpiredTokenCleanupBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ExpiredTokenCleanupBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CleanupInterval);

        do
        {
            try
            {
                await DeleteExpiredTokensAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Expired token cleanup failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task DeleteExpiredTokensAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var now = DateTime.UtcNow;

        await scope.ServiceProvider
            .GetRequiredService<IUserSessionRepository>()
            .DeleteExpiredAsync(now, cancellationToken);
        await scope.ServiceProvider
            .GetRequiredService<IPasswordResetTokenRepository>()
            .DeleteExpiredAsync(now, cancellationToken);
    }
}
