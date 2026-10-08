using Microsoft.Extensions.Options;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;
using RabbitMQ.Client;

namespace ATMS.Messaging.Infrastructure;

public sealed class RabbitMqConnectionFactory(IOptions<QueueOptions> queueOptions)
{
    private readonly QueueOptions _options = queueOptions.Value;

    private IConnection? _connection;
    
    // Without a lock, you'll get two connections instead of one.
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true, // automatically reconnect
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }
}
