using RabbitMQ.Client;

namespace ATMS.Messaging.Infrastructure.Initializers;

public static class QueueInitializerHelper
{
    public static async Task DeclareQueueSetAsync(
        IChannel channel,
        string mainQueue,
        string retryQueue,
        string deadQueue,
        string mainExchange,
        string deadExchange,
        string routingKey,
        int retryDelayMs = 30_000,
        CancellationToken cancellationToken = default)
    {
        // Dead letter queue
        await channel.QueueDeclareAsync(deadQueue,
            durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        
        await channel.QueueBindAsync(deadQueue, deadExchange, routingKey, cancellationToken: cancellationToken);

        // Retry queue (Waits 30 seconds and then comes back to main)
        await channel.QueueDeclareAsync(retryQueue,
            durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-message-ttl"] = retryDelayMs, // 30 seconds delay
                ["x-dead-letter-exchange"] = mainExchange, // Where to go after TTL
                ["x-dead-letter-routing-key"] = routingKey
            }, cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(mainQueue,
            durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = deadExchange,
                ["x-dead-letter-routing-key"] = routingKey
            }, cancellationToken: cancellationToken);
        
        await channel.QueueBindAsync(mainQueue, mainExchange, routingKey, cancellationToken: cancellationToken);
    }
}