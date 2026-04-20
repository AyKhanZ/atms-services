using ATMS.Messaging.Infrastructure;
using ATMS.Messaging.Modules;
using ATMS.Project.Services.Consumers.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class EventMessagesModule
{
    public static IServiceCollection AddMessageServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMessagingServices(configuration);
        
        services.AddSingleton<UserCreatedConsumer>();
        services.AddHostedService<ConsumerHostedService<UserCreatedConsumer>>();
        //
        // services.AddSingleton<UserUpdatedConsumer>();
        // services.AddHostedService<ConsumerHostedService<UserUpdatedConsumer>>();
        
        return services;
    }
}