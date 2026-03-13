using ATMS.Admin.Service.Security;
using ATMS.Admin.Service.Security.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class SecurityModule
{
    public static IServiceCollection AddSecurityServices(
        this IServiceCollection services)
    {
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAccessTokenService, AccessTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IBlackListService, BlackListService>();

        services.AddScoped<IPasswordHasherService, PasswordHasherService>();

        return services;
    }
}
