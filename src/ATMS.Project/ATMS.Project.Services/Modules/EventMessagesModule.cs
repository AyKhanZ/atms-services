using ATMS.Messaging.Infrastructure;
using ATMS.Messaging.Modules;
using ATMS.Project.Services.Consumers.Users;
using ATMS.Project.Services.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class EventMessagesModule
{
    public static IServiceCollection AddMessageServices(this IServiceCollection services)
    {
        services.AddMessagingServices();
        
        services.AddSingleton<UserCreatedConsumer>();
        services.AddHostedService<ConsumerHostedService<UserCreatedConsumer>>();
        
        services.AddSingleton<UserUpdatedConsumer>();
        services.AddHostedService<ConsumerHostedService<UserUpdatedConsumer>>();

        services.AddSingleton<UserStatusChangedConsumer>();
        services.AddHostedService<ConsumerHostedService<UserStatusChangedConsumer>>();

        services.AddSingleton<DeliveryRetrySchedule>();
        services.AddHostedService<OutboxBackgroundService>();
        services.AddHostedService<MessageRetentionBackgroundService>();
        
        return services;
    }
}
