using ATMS.Admin.Data.Modules;
using ATMS.Admin.Service.Providers;
using ATMS.Admin.Service.Providers.Interfaces;
using ATMS.Email.Modules;
using ATMS.Application.Dispatcher.Modules;
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
        services.AddValidationServices();
        services.AddAdminData(configuration);
        services.AddEmailServices(configuration);
        services.AddSecurityServices();
        services.AddHandlerServices();
        services.AddMapperServices();
        
        services.AddHttpClient<IOrganizationProvider, OrganizationsProvider>(client =>
        {
            client.BaseAddress = new Uri("https://your-org-service-url");
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }
}
