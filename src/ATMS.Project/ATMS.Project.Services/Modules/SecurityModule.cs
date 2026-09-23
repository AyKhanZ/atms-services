using ATMS.Project.Services.Security;
using ATMS.Project.Services.Security.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class SecurityModule
{
    public static IServiceCollection AddProjectSecurityServices(
        this IServiceCollection services)
    {
        services.AddScoped<IProjectPermissionService, ProjectPermissionService>();
        services.AddScoped<IProjectAccessPolicyResolver, ProjectAccessPolicyResolver>();

        return services;
    }
}
