using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Exceptions.Resources;
using ATMS.Project.Services.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Project.Services.Modules;

public static class TimeModule
{
    public static IServiceCollection AddTimeServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var id = configuration["BusinessTimeZone"];
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ConfigurationException(
                ConfigurationErrorTypeEnum.BusinessTimeZoneNotFound,
                string.Format(LogMessages.ConfigSectionNotFound, "BusinessTimeZone"));
        }

        TimeZoneInfo zone;
        try
        {
            zone = TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (Exception exception) when (exception is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new ConfigurationException(
                ConfigurationErrorTypeEnum.BusinessTimeZoneUnavailable,
                $"BusinessTimeZone '{id}' is unavailable on this server.");
        }

        services.AddSingleton(new BusinessTimeZone(zone));
        return services;
    }
}
