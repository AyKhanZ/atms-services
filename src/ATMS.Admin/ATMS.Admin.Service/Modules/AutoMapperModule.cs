using ATMS.Admin.Service.Mappers;
using ATMS.Admin.Service.Mappers.Actions;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class AutoMapperModule
{
    public static IServiceCollection AddMapperServices(
        this IServiceCollection services)
    {
        services.AddTransient<OnboardingModelMappingAction>();

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<CommandToEntityProfile>();
            cfg.AddProfile<EventToEntityProfile>();
            cfg.AddProfile<EntityToModelProfile>();
            cfg.AddProfile<RequestToFilterProfile>();
        });

        return services;
    }
}
