using ATMS.Application.Modules;
using ATMS.Caching.Modules;
using ATMS.Email.Modules;
using ATMS.Infrastructure.Extensions;
using ATMS.Project.Data.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ATMS.Project.Services.Security;
using ATMS.Project.Services.Security.Interfaces;

namespace ATMS.Project.Services.Modules;

public static class ProjectServicesModule
{
    public static IServiceCollection AddProjectServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMessageServices(configuration);
        services.AddRedisCache(configuration);
        services.AddCurrentUser();
        services.AddValidationServices();
        services.AddProjectData(configuration);
        services.AddScoped<IProjectPermissionService, ProjectPermissionService>();
        services.AddEmailServices(configuration);
        services.AddHandlerServices();
        services.AddMapperServices();
        services.AddLocalImageStorage();

        return services;
    }
}
