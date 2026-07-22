using ATMS.Project.Services.Mappers;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class AutoMapperModule
{
    public static IServiceCollection AddMapperServices(
        this IServiceCollection services)
    {
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
