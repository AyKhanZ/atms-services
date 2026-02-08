using ATMS.Admin.Service.Mappers;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Admin.Service.Modules;

public static class AutoMapperModule
{
    public static IServiceCollection AddMapperServices(
        this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<CommandToEntityProfile>();
            cfg.AddProfile<EntityToModelProfile>();
        });

        return services;
    }
}
