using ATMS.Messaging.Configuration;
using RabbitMQ.Client;

namespace ATMS.Messaging.Infrastructure.Initializers;

public class UserInvitedQueueInitializer
{
    public static Task InitializeAsync(IChannel channel, CancellationToken cancellationToken) =>
        QueueInitializerHelper.DeclareQueueSetAsync(
            channel,
            mainQueue:    MessagingConstants.Queues.UserInvited,
            retryQueue:   MessagingConstants.Queues.UserInvitedRetry,
            deadQueue:    MessagingConstants.Queues.UserInvitedDead,
            mainExchange: MessagingConstants.Exchanges.UserEvents,
            deadExchange: MessagingConstants.Exchanges.UserEvents + ".dead",
            routingKey:   MessagingConstants.RoutingKeys.UserInvited,
            cancellationToken: cancellationToken);
}