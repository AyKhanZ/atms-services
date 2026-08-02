using System.Globalization;
using System.Text;
using System.Text.Json;
using ATMS.Messaging.Interfaces;
using ATMS.Messaging.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ATMS.Messaging.Infrastructure;

public abstract class RabbitMqConsumerBase<T>(
    RabbitMqConnectionFactory connectionFactory,
    IServiceScopeFactory scopeFactory,
    ILogger logger,
    string queueName,
    ushort prefetchCount = 10)
    : IMessageConsumer
{
    private const int MaxAttemptCount = 10;
    private readonly TimeSpan[] _retryDelays =
    [
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(2),
        TimeSpan.FromHours(4),
        TimeSpan.FromHours(8)
    ];

    private IConnection? _connection;
    private IChannel? _channel;
    private CancellationToken _cancellationToken;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _connection = await connectionFactory.GetConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(
            new CreateChannelOptions(true, true),
            cancellationToken);
        _cancellationToken = cancellationToken;
        
        // Keeps no more than N unacknowledged messages in this consumer.
        // This prevents one slow consumer from taking the whole queue into memory.
        await _channel.BasicQosAsync(0, prefetchCount, false, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: queueName,
            // A message is acknowledged only after HandleAsync succeeds.
            // If the process stops before that, RabbitMQ can deliver it again.
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync(cancellationToken);
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync(cancellationToken);
        }
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        var messageId = args.BasicProperties.MessageId ?? "unknown";

        try
        {
            var json = Encoding.UTF8.GetString(args.Body.Span);
            var envelope = JsonSerializer.Deserialize<MessageEnvelope>(json);

            if (envelope is null)
            {
                logger.LogWarning("Received null envelope for message {MessageId}", messageId);
                await RejectAsync(args.DeliveryTag);
                return;
            }

            var expectedMessageType = typeof(T).FullName ?? typeof(T).Name;
            if (envelope.MessageType != expectedMessageType &&
                envelope.MessageType != typeof(T).Name)
            {
                throw new JsonException(
                    $"Message type {envelope.MessageType} cannot be handled as {expectedMessageType}.");
            }

            var message = envelope.Payload.Deserialize<T>()
                ?? throw new JsonException($"Message {envelope.MessageId} has an empty payload.");

            await using var scope = scopeFactory.CreateAsyncScope();
            await HandleAsync(message, envelope.MessageId, scope.ServiceProvider, _cancellationToken);
            await _channel!.BasicAckAsync(args.DeliveryTag, false, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to process message {MessageId}", messageId);

            var attemptCount = GetRetryCount(args.BasicProperties.Headers) + 1;
            if (attemptCount >= MaxAttemptCount)
            {
                // Retries are exhausted: send the original message to the queue DLX.
                await RejectAsync(args.DeliveryTag);
                return;
            }

            var retryProperties = new BasicProperties
            {
                Persistent = true,
                ContentType = args.BasicProperties.ContentType ?? "application/json",
                MessageId = args.BasicProperties.MessageId,
                Timestamp = args.BasicProperties.Timestamp,
                Expiration = ((long)_retryDelays[attemptCount - 1].TotalMilliseconds)
                    .ToString(CultureInfo.InvariantCulture),
                Headers = CopyHeaders(args.BasicProperties.Headers, attemptCount)
            };

            // Nack(requeue: false) only sends to the queue's dead-letter exchange.
            // The retry queue is separate, so publish there explicitly and acknowledge
            // the original only after RabbitMQ accepts the retry copy.
            await _channel!.BasicPublishAsync(
                exchange: "",
                routingKey: queueName + ".retry",
                mandatory: true,
                basicProperties: retryProperties,
                body: args.Body,
                cancellationToken: CancellationToken.None);
            await _channel.BasicAckAsync(args.DeliveryTag, false, CancellationToken.None);
        }
    }

    protected abstract Task HandleAsync(
        T message,
        Guid messageId,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);

    private Task RejectAsync(ulong deliveryTag)
    {
        // requeue: false lets RabbitMQ route the message to its configured DLX.
        return _channel!.BasicNackAsync(
            deliveryTag,
            false,
            requeue: false,
            CancellationToken.None).AsTask();
    }

    private static Dictionary<string, object?> CopyHeaders(
        IDictionary<string, object?>? headers,
        int retryCount)
    {
        var copy = headers is null
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?>(headers);
        copy["x-retry-count"] = retryCount;
        return copy;
    }

    private static int GetRetryCount(IDictionary<string, object?>? headers)
    {
        if (headers is null || !headers.TryGetValue("x-retry-count", out var value))
        {
            return 0;
        }

        return value switch
        {
            int count when count >= 0 => count,
            long count when count is >= 0 and <= int.MaxValue => (int)count,
            byte count => count,
            byte[] bytes when int.TryParse(Encoding.UTF8.GetString(bytes), out var count) &&
                              count >= 0 => count,
            _ => 0
        };
    }
}
