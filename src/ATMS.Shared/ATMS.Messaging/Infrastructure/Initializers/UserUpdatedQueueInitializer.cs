using ATMS.Messaging.Configuration;
using RabbitMQ.Client;

namespace ATMS.Messaging.Infrastructure.Initializers;

public static class UserUpdatedQueueInitializer
{
    public static Task InitializeAsync(IChannel channel, CancellationToken cancellationToken) =>
        QueueInitializerHelper.DeclareQueueSetAsync(
            channel,
            mainQueue: MessagingConstants.Queues.ProjectUserUpdated,
            retryQueue: MessagingConstants.Queues.ProjectUserUpdatedRetry,
            deadQueue: MessagingConstants.Queues.ProjectUserUpdatedDead,
            mainExchange: MessagingConstants.Exchanges.UserEvents,
            deadExchange: MessagingConstants.Exchanges.UserEvents + ".dead",
            routingKey: MessagingConstants.RoutingKeys.UserUpdated,
            cancellationToken: cancellationToken);
}