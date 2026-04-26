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
    private IConnection? _connection;
    private IChannel? _channel;
    private CancellationToken _cancellationToken;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _connection = await connectionFactory.GetConnectionAsync();
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
        _cancellationToken = cancellationToken;
        
        // prefetch — do not take more than N messages at a time, overload protection
        await _channel.BasicQosAsync(0, prefetchCount, false, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false, // IMPORTANT: manual ack, otherwise we will lose the message when falling
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
            var envelope = JsonSerializer.Deserialize<MessageEnvelope<T>>(json);

            if (envelope is null)
            {
                logger.LogWarning("Received null envelope for message {MessageId}", messageId);
                await _channel!.BasicNackAsync(args.DeliveryTag, false, requeue: false, CancellationToken.None);
                return;
            }
            await using var scope = scopeFactory.CreateAsyncScope();
            await HandleAsync(envelope.Payload, scope.ServiceProvider, _cancellationToken);

            await _channel!.BasicAckAsync(args.DeliveryTag, false, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process message {MessageId}", messageId);

            // requeue: false — DO NOT return to the same queue
            // RabbitMQ will send it to DLX itself if it is configured
            await _channel!.BasicNackAsync(args.DeliveryTag, false, requeue: false, CancellationToken.None);
        }
    }

    protected abstract Task HandleAsync(T message, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}