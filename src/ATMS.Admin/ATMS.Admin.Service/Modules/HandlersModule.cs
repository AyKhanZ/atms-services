using ATMS.Admin.Service.Behaviors;
using MediatR;
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

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>)
        );

        return services;
    }
}