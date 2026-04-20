using ATMS.Application.Modules;
using ATMS.Email.Modules;
using ATMS.Project.Data.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class ProjectServicesModule
{
    public static IServiceCollection AddProjectServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMessageServices(configuration);
        services.AddCurrentUser();
        services.AddValidationServices();
        services.AddProjectData(configuration);
        services.AddEmailServices(configuration);
        services.AddHandlerServices();
        services.AddMapperServices();

        return services;
    }
}