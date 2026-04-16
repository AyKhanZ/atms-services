using ATMS.Admin.Data.Modules;
using ATMS.Email.Modules;
using ATMS.Application.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class AdminServicesModule
{
    public static IServiceCollection AddAdminServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureServices();
        services.AddCurrentUser();
        services.AddProviderServices(configuration);
        services.AddValidationServices();
        services.AddAdminData(configuration);
        services.AddEmailServices(configuration);
        services.AddSecurityServices();
        services.AddHandlerServices();
        services.AddMapperServices();

        return services;
    }
}