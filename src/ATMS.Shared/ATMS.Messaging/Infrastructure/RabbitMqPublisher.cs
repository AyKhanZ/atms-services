using System.Text;
using System.Text.Json;
using ATMS.Messaging.Interfaces;
using ATMS.Messaging.Models;
using RabbitMQ.Client;

namespace ATMS.Messaging.Infrastructure;

public sealed class RabbitMqPublisher(RabbitMqConnectionFactory connectionFactory) : IMessagePublisher
{
    public async Task PublishAsync<T>(
        string exchange,
        string routingKey,
        T message,
        CancellationToken cancellationToken = default)
    {
        var envelope = new MessageEnvelope(
            Guid.NewGuid(),
            DateTime.UtcNow,
            typeof(T).FullName ?? typeof(T).Name,
            JsonSerializer.SerializeToElement(message));

        await PublishEnvelopeAsync(exchange, routingKey, envelope, cancellationToken);
    }

    public async Task PublishAsync(
        string exchange,
        string routingKey,
        string messageType,
        string payload,
        Guid messageId,
        DateTime createdAt,
        CancellationToken cancellationToken = default)
    {
        var envelope = new MessageEnvelope(
            messageId,
            createdAt,
            messageType,
            JsonSerializer.Deserialize<JsonElement>(payload));

        await PublishEnvelopeAsync(exchange, routingKey, envelope, cancellationToken);
    }

    private async Task PublishEnvelopeAsync(
        string exchange,
        string routingKey,
        MessageEnvelope envelope,
        CancellationToken cancellationToken)
    {
        var connection = await connectionFactory.GetConnectionAsync(cancellationToken);
        var channelOptions = new CreateChannelOptions(true, true);
        await using var channel = await connection.CreateChannelAsync(channelOptions, cancellationToken);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));
        var props = new BasicProperties
        {
            Persistent = true, // the message will survive after the broker restart
            ContentType = "application/json",
            MessageId = envelope.MessageId.ToString(),
            Timestamp = new AmqpTimestamp(new DateTimeOffset(envelope.CreatedAt).ToUnixTimeSeconds())
        };

        // With publisher confirmation tracking enabled this call completes only
        // after RabbitMQ confirms the publish or reports that it cannot be routed.
        await channel.BasicPublishAsync(
            exchange: exchange,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);
    }
}
