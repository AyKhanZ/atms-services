using ATMS.Application.Dispatcher.Modules;
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
        services.AddDispatcherServices();

        return services;
    }
}
