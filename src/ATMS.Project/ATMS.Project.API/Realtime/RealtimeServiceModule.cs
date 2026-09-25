using ATMS.Application.Realtime;
using ATMS.Project.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ATMS.Project.API.Realtime;

public static class RealtimeServiceModule
{
    public static IServiceCollection AddRealtime(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, SubClaimUserIdProvider>();
        services.AddSingleton<IRealtimeEventPublisher, SignalRRealtimeEventPublisher>();
        return services;
    }
}
