using ATMS.Application.Infrastructure;
using ATMS.Application.Interfaces;
using ATMS.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Application.Modules;

public static class CurrentUserModule
{
    public static IServiceCollection AddCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IAuditActorAccessor>(provider => (IAuditActorAccessor)provider.GetRequiredService<ICurrentUser>());
        services.AddSingleton<IDateTimeDisplayService, DateTimeDisplayService>();
        services.AddTransient<AuthorizationDelegatingHandler>();
        
        return services;
    }
}
