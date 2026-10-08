using ATMS.Project.Services.Domain.History;
using ATMS.Project.Services.Domain.History.Interfaces;
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
