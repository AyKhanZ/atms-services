using ATMS.Messaging.Infrastructure;
using ATMS.Messaging.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Messaging.Modules;

public static class MessagingModule
{
    public static IServiceCollection AddMessagingServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var factory = new RabbitMqConnectionFactory(configuration);
        
        services.AddSingleton(factory);
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
        services.AddSingleton<MessagingConstantsInitializer>();
        
        return services;
    }
}