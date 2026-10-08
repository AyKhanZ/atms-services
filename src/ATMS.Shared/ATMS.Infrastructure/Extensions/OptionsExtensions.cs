using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Infrastructure.Extensions;

public static class OptionsExtensions
{
    // section name = class name; a missing or empty [Required] value stops the app at start, not on first request
    public static IServiceCollection AddRequiredOptions<TOptions>(this IServiceCollection services)
        where TOptions : class
    {
        services.AddOptions<TOptions>()
            .BindConfiguration(typeof(TOptions).Name)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
