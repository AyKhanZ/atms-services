using ATMS.Project.Services.History;
using ATMS.Project.Services.History.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class HistoryModule
{
    public static IServiceCollection AddHistoryServices(
        this IServiceCollection services)
    {
        services.AddScoped<IHistoryScopeService, HistoryScopeService>();
        services.AddScoped<IHistoryValueResolver, HistoryValueResolver>();

        return services;
    }
}
