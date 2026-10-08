using ATMS.Admin.Service.Security;
using ATMS.Admin.Service.Infrastructure;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Infrastructure.Extensions;
using ATMS.Infrastructure.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class SecurityModule
{
    public static IServiceCollection AddSecurityServices(
        this IServiceCollection services)
    {
        services.AddRequiredOptions<JwtOptions>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IAccessTokenService, AccessTokenService>();
        services.AddScoped<IUniqueTokenService, UniqueTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IResetPasswordTokenService, ResetPasswordTokenService>();
        services.AddScoped<IEmailConfirmationTokenService, EmailConfirmationTokenService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();

        services.AddHostedService<ExpiredTokenCleanupBackgroundService>();

        return services;
    }

    public static IServiceCollection AddCompletedOnboardingBehavior(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CompletedOnboardingBehavior<,>));

        return services;
    }
}
