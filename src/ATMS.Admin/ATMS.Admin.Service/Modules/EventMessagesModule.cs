using ATMS.Admin.Service.Consumers.Users;
using ATMS.Messaging.Infrastructure;
using ATMS.Messaging.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class EventMessagesModule
{
    public static IServiceCollection AddMessageServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMessagingServices(configuration);
        
        services.AddSingleton<UserInvitedConsumer>();
        services.AddHostedService<ConsumerHostedService<UserInvitedConsumer>>();
        
        return services;
    }
}