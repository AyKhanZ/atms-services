using ATMS.Admin.Service.Infrastructure;
using ATMS.Admin.Service.Infrastructure.Interfaces;
using ATMS.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services)
    {
        services.AddScoped<IDataInitializer, DataInitializer>();
        services.AddLocalImageStorage();
        
        return services;
    }
}
