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
        var connection = await connectionFactory.GetConnectionAsync();
        var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await using (channel)
        {
            var envelope = new MessageEnvelope<T> { Payload = message };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));

            var props = new BasicProperties
            {
                Persistent = true, // the message will survive after the broker restart
                ContentType = "application/json",
                MessageId = envelope.MessageId.ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: true, // if there is no queue, it will return an error, it will not lose it quietly
                basicProperties: props,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}