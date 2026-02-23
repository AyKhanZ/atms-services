using ATMS.Admin.Data.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class AdminServicesModule
{
    public static IServiceCollection AddAdminServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidationServices();
        services.AddAdminData(configuration);
        services.AddSecurityServices();
        services.AddHandlerServices();
        services.AddMapperServices();

        return services;
    }
}
