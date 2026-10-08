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
        // order matters: 403, then 409, then validation
        services.AddLocalizationBehavior();
        services.AddAccessBehavior();
        services.AddCompletedOnboardingBehavior();
        services.AddSharedValidationServices();
        services.AddValidationBehavior();

        return services;
    }
}
