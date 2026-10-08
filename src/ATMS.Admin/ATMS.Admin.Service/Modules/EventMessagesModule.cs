using ATMS.Admin.Service.Consumers.Users;
using ATMS.Admin.Service.Infrastructure.Delivery;
using ATMS.Infrastructure.Extensions;
using ATMS.Infrastructure.Options;
using ATMS.Messaging.Infrastructure;
using ATMS.Messaging.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class EventMessagesModule
{
    public static IServiceCollection AddMessageServices(this IServiceCollection services)
    {
        services.AddMessagingServices();
        services.AddRequiredOptions<RedirectUrlOptions>();
        
        services.AddSingleton<UserInvitedConsumer>();
        services.AddHostedService<ConsumerHostedService<UserInvitedConsumer>>();

        services.AddSingleton<DeliveryRetrySchedule>();
        services.AddSingleton<EmailDeliveryRequestLock>();
        services.AddHostedService<OutboxBackgroundService>();
        services.AddHostedService<EmailDeliveryBackgroundService>();
        services.AddHostedService<DeliveryRetentionBackgroundService>();
        
        return services;
    }
}
