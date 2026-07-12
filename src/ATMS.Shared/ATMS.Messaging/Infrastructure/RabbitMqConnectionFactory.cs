using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace ATMS.Messaging.Infrastructure;

public sealed class RabbitMqConnectionFactory(IConfiguration configuration)
{
    private readonly QueueOptions _options = configuration.GetSection(nameof(QueueOptions)).Get<QueueOptions>()
                                             ?? throw new ConfigurationException(ConfigurationErrorType.DatabaseSectionNotFound,
                                                 string.Format(LogMessages.ConfigSectionNotFound, nameof(QueueOptions)));

    private IConnection? _connection;
    
    // Without a lock, you'll get two connections instead of one.
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<IConnection> GetConnectionAsync()
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        await _lock.WaitAsync();
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

            _connection = await factory.CreateConnectionAsync();
            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }
}