using ATMS.Application.Dispatcher.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class HandlersModule
{
    public static IServiceCollection AddHandlerServices(
        this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(HandlersModule).Assembly);
        });
        // Order is the pipeline order: a super admin gets 403 before the onboarding 409, and both before validation.
        services.AddLocalizationBehavior();
        services.AddAccessBehavior();
        services.AddCompletedOnboardingBehavior();
        services.AddSharedValidationServices();
        services.AddValidationBehavior();

        return services;
    }
}
