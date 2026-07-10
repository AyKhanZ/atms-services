using ATMS.Messaging.Configuration;
using ATMS.Messaging.Infrastructure.Initializers;
using RabbitMQ.Client;

namespace ATMS.Messaging.Infrastructure;

public class MessagingInitializer(RabbitMqConnectionFactory connectionFactory)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var connection = await connectionFactory.GetConnectionAsync();
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await using (channel)
        {
            await channel.ExchangeDeclareAsync(
                MessagingConstants.Exchanges.UserEvents,
                ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);

            await channel.ExchangeDeclareAsync(
                MessagingConstants.Exchanges.UserEvents + ".dead",
                ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);

            await UserCreatedQueueInitializer.InitializeAsync(channel, cancellationToken);
            await UserUpdatedQueueInitializer.InitializeAsync(channel, cancellationToken);
            await UserInvitedQueueInitializer.InitializeAsync(channel, cancellationToken);
        }
    }
}