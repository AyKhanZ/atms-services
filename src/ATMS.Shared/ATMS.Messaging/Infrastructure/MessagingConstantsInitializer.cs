using ATMS.Messaging.Configuration;
using RabbitMQ.Client;

namespace ATMS.Messaging.Infrastructure;

public class MessagingConstantsInitializer(RabbitMqConnectionFactory connectionFactory)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var connection = await connectionFactory.GetConnectionAsync();
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await using (channel)
        {
            // 1. Dead letter exchange
            await channel.ExchangeDeclareAsync(
                exchange: MessagingConstants.Exchanges.UserEvents + ".dead",
                type: ExchangeType.Direct,
                durable: true,
                cancellationToken: cancellationToken);

            // 2. Dead letter queue
            await channel.QueueDeclareAsync(
                queue: MessagingConstants.Queues.ProjectUserCreatedDead,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: MessagingConstants.Queues.ProjectUserCreatedDead,
                exchange: MessagingConstants.Exchanges.UserEvents + ".dead",
                routingKey: MessagingConstants.RoutingKeys.UserCreated,
                cancellationToken: cancellationToken);

            // 3. Retry queue (Waits 30 seconds and then comes back to main)
            var retryArgs = new Dictionary<string, object?>
            {
                ["x-message-ttl"] = 30_000,                                           // 30 seconds delay
                ["x-dead-letter-exchange"] = MessagingConstants.Exchanges.UserEvents,   // where to go after TTL
                ["x-dead-letter-routing-key"] = MessagingConstants.RoutingKeys.UserCreated
            };

            await channel.QueueDeclareAsync(
                queue: MessagingConstants.Queues.ProjectUserCreatedRetry,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: retryArgs,
                cancellationToken: cancellationToken);

            // 4. Main exchange
            await channel.ExchangeDeclareAsync(
                exchange: MessagingConstants.Exchanges.UserEvents,
                type: ExchangeType.Direct,
                durable: true,
                cancellationToken: cancellationToken);

            // 5. Main queue with DLX
            var mainArgs = new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = MessagingConstants.Exchanges.UserEvents + ".dead",
                ["x-dead-letter-routing-key"] = MessagingConstants.RoutingKeys.UserCreated
            };

            await channel.QueueDeclareAsync(
                queue: MessagingConstants.Queues.ProjectUserCreated,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: mainArgs,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: MessagingConstants.Queues.ProjectUserCreated,
                exchange: MessagingConstants.Exchanges.UserEvents,
                routingKey: MessagingConstants.RoutingKeys.UserCreated,
                cancellationToken: cancellationToken);
        }
    }
}