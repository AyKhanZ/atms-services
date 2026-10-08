using ATMS.Application.Exceptions.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Infrastructure.Extensions;

public static class OptionsExtensions
{
    // section name = class name; no section in appsettings and the app doesnt start, instead of failing on first request
    public static IServiceCollection AddRequiredOptions<TOptions>(this IServiceCollection services)
        where TOptions : class
    {
        var section = typeof(TOptions).Name;

        services.AddOptions<TOptions>()
            .BindConfiguration(section)
            .Validate<IConfiguration>(
                (_, configuration) => configuration.GetSection(section).Exists(),
                string.Format(LogMessages.ConfigSectionNotFound, section))
            .ValidateOnStart();

        return services;
    }
}
