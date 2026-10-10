using System.ComponentModel.DataAnnotations;
using ATMS.Infrastructure.Options;
using ATMS.Swagger.Constants;
using ATMS.Swagger.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ATMS.Swagger.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimitingPolicies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var limits = ReadOptions(configuration);
        var partitions = new RateLimitPartitions(limits);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = partitions.Global();
            options.OnRejected = RateLimitRejection.WriteAsync;

            // before sign-in, so always by IP
            options.AddPolicy(RateLimitPolicies.AuthLogin, context => partitions.SlidingWindow(
                context, RateLimitPolicies.AuthLogin, RateLimitPartitions.IpKey(context), limits.AuthLogin));
            options.AddPolicy(RateLimitPolicies.AuthRefresh, context => partitions.SlidingWindow(
                context, RateLimitPolicies.AuthRefresh, RateLimitPartitions.IpKey(context), limits.AuthRefresh));
            options.AddPolicy(RateLimitPolicies.AuthEmail, context => partitions.FixedWindow(
                context, RateLimitPolicies.AuthEmail, RateLimitPartitions.IpKey(context), limits.AuthEmail));
            options.AddPolicy(RateLimitPolicies.AuthToken, context => partitions.FixedWindow(
                context, RateLimitPolicies.AuthToken, RateLimitPartitions.IpKey(context), limits.AuthToken));

            // the daily quota of these three is added by the global limiter
            options.AddPolicy(RateLimitPolicies.Creates, context => partitions.TokenBucket(
                context, RateLimitPolicies.Creates, RateLimitPartitions.Key(context), limits.Creates));
            options.AddPolicy(RateLimitPolicies.Emails, context => partitions.TokenBucket(
                context, RateLimitPolicies.Emails, RateLimitPartitions.Key(context), limits.Emails));
            options.AddPolicy(RateLimitPolicies.Uploads, context => partitions.TokenBucket(
                context, RateLimitPolicies.Uploads, RateLimitPartitions.Key(context), limits.Uploads));

            options.AddPolicy(RateLimitPolicies.Downloads, context => partitions.Concurrency(
                context, RateLimitPolicies.Downloads, RateLimitPartitions.Key(context), limits.Downloads));
            options.AddPolicy(RateLimitPolicies.Heavy, context => partitions.Concurrency(
                context, RateLimitPolicies.Heavy, RateLimitPartitions.Key(context), limits.Heavy));
            // two migrations of one database at once break it, so one counter for the whole service
            options.AddPolicy(RateLimitPolicies.Migrations, context => partitions.Concurrency(
                context, RateLimitPolicies.Migrations, "service", limits.Migrations));
        });

        services.AddSingleton<RejectionLogThrottle>();

        return services;
    }

    // X-Forwarded-For is taken only from the proxies listed here, otherwise anyone could fake their IP
    public static WebApplication UseProxyForwardedHeaders(this WebApplication app, IConfiguration configuration)
    {
        var proxyOptions = configuration.GetSection(nameof(ProxyOptions)).Get<ProxyOptions>();
        var forwardedHeaders = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };

        foreach (var network in proxyOptions?.KnownNetworks ?? [])
        {
            forwardedHeaders.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
        }

        app.UseForwardedHeaders(forwardedHeaders);
        return app;
    }

    // read once at start; DataAnnotations does not look into nested objects, so every level is checked here
    internal static RateLimitOptions ReadOptions(IConfiguration configuration)
    {
        var options = configuration.GetSection(nameof(RateLimitOptions)).Get<RateLimitOptions>();
        var failures = new List<string>();
        if (options is null)
        {
            failures.Add($"{nameof(RateLimitOptions)} section is missing.");
        }
        else
        {
            Validate(options, nameof(RateLimitOptions), failures);
        }

        if (failures.Count > 0)
        {
            throw new OptionsValidationException(nameof(RateLimitOptions), typeof(RateLimitOptions), failures);
        }

        return options!;
    }

    private static void Validate(object target, string path, List<string> failures)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(target, new ValidationContext(target), results, validateAllProperties: true);
        failures.AddRange(results.Select(result =>
            $"{path}:{string.Join(",", result.MemberNames)}: {result.ErrorMessage}"));

        foreach (var property in target.GetType().GetProperties())
        {
            if (property.PropertyType.IsClass && property.GetValue(target) is { } child)
            {
                Validate(child, $"{path}:{property.Name}", failures);
            }
        }
    }
}
