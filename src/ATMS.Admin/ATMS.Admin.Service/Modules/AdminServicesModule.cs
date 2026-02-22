using ATMS.Admin.Data.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class AdminServicesModule
{
    public static IServiceCollection AddAdminServices(
        this IServiceCollection services)
    {
        services.AddValidationServices();
        services.AddAdminData();
        services.AddSecurityServices();
        services.AddHandlerServices();
        services.AddMapperServices();

        return services;
    }
}
