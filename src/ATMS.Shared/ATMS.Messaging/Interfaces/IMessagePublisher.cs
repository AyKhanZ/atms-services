namespace ATMS.Messaging.Interfaces;

// routing key: user.created

public interface IMessagePublisher
{
    Task PublishAsync<T>(
        string exchange,
        string routingKey,
        T message,
        CancellationToken cancellationToken = default);
}