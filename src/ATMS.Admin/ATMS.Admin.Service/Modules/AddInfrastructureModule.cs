using ATMS.Admin.Service.Infrastructure;
using ATMS.Admin.Service.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class AddInfrastructureModule
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services)
    {
        services.AddScoped<IDataInitializer, DataInitializer>();
        
        return services;
    }
}
