using ATMS.Application.Dispatcher.Modules;
using ATMS.Project.Services.Dispatcher.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class HandlersModule
{
    public static IServiceCollection AddHandlerServices(
        this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(HandlersModule).Assembly);
        });
        services.AddLocalizationBehavior();
        services.AddAccessBehavior();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ProjectAccessBehavior<,>));
        services.AddSharedValidationServices();
        services.AddValidationBehavior();

        return services;
    }
}
