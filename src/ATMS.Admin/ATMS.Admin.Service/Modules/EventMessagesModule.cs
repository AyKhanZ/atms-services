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
        
        return services;
    }
}