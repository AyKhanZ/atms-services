using ATMS.Infrastructure.Extensions;
using ATMS.Infrastructure.Options;
using ATMS.Messaging.Infrastructure;
using ATMS.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Messaging.Modules;

public static class MessagingModule
{
    public static IServiceCollection AddMessagingServices(this IServiceCollection services)
    {
        services.AddRequiredOptions<QueueOptions>();
        services.AddSingleton<RabbitMqConnectionFactory>();
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
        services.AddSingleton<MessagingInitializer>();

        return services;
    }
}
