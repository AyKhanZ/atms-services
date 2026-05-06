using ATMS.Messaging.Configuration;
using RabbitMQ.Client;

namespace ATMS.Messaging.Infrastructure.Initializers;

public static class UserCreatedQueueInitializer
{
    public static Task InitializeAsync(IChannel channel, CancellationToken cancellationToken) =>
        QueueInitializerHelper.DeclareQueueSetAsync(
            channel,
            mainQueue:    MessagingConstants.Queues.ProjectUserCreated,
            retryQueue:   MessagingConstants.Queues.ProjectUserCreatedRetry,
            deadQueue:    MessagingConstants.Queues.ProjectUserCreatedDead,
            mainExchange: MessagingConstants.Exchanges.UserEvents,
            deadExchange: MessagingConstants.Exchanges.UserEvents + ".dead",
            routingKey:   MessagingConstants.RoutingKeys.UserCreated,
            cancellationToken: cancellationToken);
}