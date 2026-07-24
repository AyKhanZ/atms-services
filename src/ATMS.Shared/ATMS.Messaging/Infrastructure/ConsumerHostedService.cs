using ATMS.Messaging.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ATMS.Messaging.Infrastructure;

public sealed class ConsumerHostedService<TConsumer>(
    TConsumer consumer,
    MessagingInitializer messagingInitializer,
    ILogger<ConsumerHostedService<TConsumer>> logger)
    : BackgroundService where TConsumer : IMessageConsumer
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await messagingInitializer.InitializeAsync(stoppingToken);
                await consumer.StartAsync(stoppingToken);
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "RabbitMQ consumer {ConsumerName} could not start. Retrying in {RetryDelaySeconds} seconds",
                    typeof(TConsumer).Name,
                    RetryDelay.TotalSeconds);
                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await consumer.StopAsync(cancellationToken);
    }
}
