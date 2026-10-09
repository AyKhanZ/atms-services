using ATMS.Admin.Service.Infrastructure;
using ATMS.Admin.Service.Infrastructure.Interfaces;
using ATMS.Infrastructure.Extensions;
using ATMS.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services)
    {
        services.AddRequiredOptions<AdminOptions>();
        services.AddScoped<IDefaultUserLanguage, DefaultUserLanguage>();
        services.AddScoped<IDataInitializer, DataInitializer>();
        services.AddLocalImageStorage();
        
        return services;
    }
}
