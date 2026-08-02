namespace ATMS.Messaging.Interfaces;

// routing key: user.created

public interface IMessagePublisher
{
    Task PublishAsync<T>(
        string exchange,
        string routingKey,
        T message,
        CancellationToken cancellationToken = default);

    Task PublishAsync(
        string exchange,
        string routingKey,
        string messageType,
        string payload,
        Guid messageId,
        DateTime createdAt,
        CancellationToken cancellationToken = default);
}
