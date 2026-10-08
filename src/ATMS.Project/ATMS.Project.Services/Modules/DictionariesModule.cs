using ATMS.Project.Services.Domain.Dictionaries;
using ATMS.Project.Services.Domain.Dictionaries.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class DictionariesModule
{
    public static IServiceCollection AddDictionaryServices(
        this IServiceCollection services)
    {
        services.AddScoped<IDictionaryCacheService, DictionaryCacheService>();

        return services;
    }
}
