using ATMS.Application.Dispatcher.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Application.Dispatcher.Modules;

public static class DispatcherModule
{
    public static IServiceCollection AddDispatcherServices(
        this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LocalizationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
