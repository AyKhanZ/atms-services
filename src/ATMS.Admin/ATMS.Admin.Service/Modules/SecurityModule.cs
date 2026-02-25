using ATMS.Admin.Service.Security;
using ATMS.Admin.Service.Security.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class SecurityModule
{
    public static IServiceCollection AddSecurityServices(
        this IServiceCollection services)
    {
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<ITokenService, TokenService>();

        services.AddScoped<IPasswordHasherService, PasswordHasherService>();

        return services;
    }
}
