using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Caching.Services;
using ATMS.Caching.Services.Interfaces;
using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATMS.Caching.Modules;

public static class CacheModule
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisOptions = configuration.GetSection(nameof(RedisOptions)).Get<RedisOptions>()
                      ?? throw new ConfigurationException(ConfigurationErrorType.DatabaseSectionNotFound,
                          string.Format(ExceptionMessages.ConfigSectionNotFound, nameof(RedisOptions)));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisOptions.ConnectionString;
            options.InstanceName =  redisOptions.InstanceName;
        });

        services.AddSingleton<ICacheService, RedisCacheService>();
        return services;
    }
}