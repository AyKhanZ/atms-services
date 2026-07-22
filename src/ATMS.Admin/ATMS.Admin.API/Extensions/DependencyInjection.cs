using ATMS.Application.Constants;

namespace ATMS.Admin.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("OnboardingAccess", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(CustomClaimTypes.OnboardingCompleted, "false");
            });

            options.AddPolicy("AdminPolicy", policy =>
            {
                policy.RequireRole("Admin");
            });
        });

        return services;
    }
}
