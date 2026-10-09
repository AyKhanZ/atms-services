using ATMS.Messaging.Configuration;
using RabbitMQ.Client;

namespace ATMS.Messaging.Infrastructure.Initializers;

public static class UserStatusChangedQueueInitializer
{
    public static Task InitializeAsync(IChannel channel, CancellationToken cancellationToken) =>
        QueueInitializerHelper.DeclareQueueSetAsync(
            channel,
            mainQueue: MessagingConstants.Queues.ProjectUserStatusChanged,
            retryQueue: MessagingConstants.Queues.ProjectUserStatusChangedRetry,
            deadQueue: MessagingConstants.Queues.ProjectUserStatusChangedDead,
            mainExchange: MessagingConstants.Exchanges.UserEvents,
            deadExchange: MessagingConstants.Exchanges.UserEvents + ".dead",
            routingKey: MessagingConstants.RoutingKeys.UserStatusChanged,
            cancellationToken: cancellationToken);
}
