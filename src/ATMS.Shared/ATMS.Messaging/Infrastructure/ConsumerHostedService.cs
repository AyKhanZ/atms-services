using ATMS.Messaging.Interfaces;
using Microsoft.Extensions.Hosting;

namespace ATMS.Messaging.Infrastructure;

public sealed class ConsumerHostedService<TConsumer>(TConsumer consumer)
    : IHostedService where TConsumer : IMessageConsumer
{
    public Task StartAsync(CancellationToken cancellationToken)
        => consumer.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken)
        => consumer.StopAsync(cancellationToken);
}