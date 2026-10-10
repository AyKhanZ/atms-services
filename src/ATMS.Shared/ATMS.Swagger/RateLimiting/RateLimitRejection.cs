using System.Globalization;
using System.Threading.RateLimiting;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Localization;
using ATMS.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace ATMS.Swagger.RateLimiting;

// the 429 answer: Retry-After, the same { error } body as other errors, one log line per key a minute
internal static class RateLimitRejection
{
    internal const string LogCategory = "ATMS.RateLimiting";

    public static async ValueTask WriteAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        var retryAfterSeconds = RetryAfterSeconds(
            context.Lease,
            httpContext.Items[RateLimitPartitions.RetryAfterItem] as TimeSpan?);
        httpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString(CultureInfo.InvariantCulture);

        var policy = httpContext.Items[RateLimitPartitions.PolicyItem] as string ?? "unknown";
        var key = httpContext.Items[RateLimitPartitions.KeyItem] as string ?? "unknown";

        // a script hitting the limit would otherwise write a line per request
        if (httpContext.RequestServices.GetRequiredService<RejectionLogThrottle>().ShouldLog(policy, key))
        {
            httpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger(LogCategory)
                .LogWarning(
                    "Rate limit rejected. Policy: {Policy}, Key: {Key}, Method: {Method}, Path: {Path}",
                    policy,
                    key,
                    httpContext.Request.Method,
                    httpContext.Request.Path);
        }

        var body = JsonConvert.SerializeObject(new { error = Message(httpContext, retryAfterSeconds) });
        httpContext.Response.ContentType = "application/json";
        await httpContext.Response.WriteAsync(body, cancellationToken);
    }

    internal static int RetryAfterSeconds(RateLimitLease lease, TimeSpan? fallback = null)
    {
        // sliding window reports zero, the caller passes its segment length instead
        var retryAfter = lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan reported) && reported > TimeSpan.Zero
            ? reported
            : fallback ?? TimeSpan.Zero;

        var seconds = (int)Math.Ceiling(retryAfter.TotalSeconds);
        return seconds < 1 ? 1 : seconds;
    }

    // fixed windows report the whole window, so a daily quota says 86400 s, never "in 3 s"
    internal static string TooManyRequestsText(int retryAfterSeconds) => retryAfterSeconds switch
    {
        < 60 => string.Format(CultureInfo.InvariantCulture, ExceptionMessages.TooManyRequests, retryAfterSeconds),
        < 3600 => string.Format(CultureInfo.InvariantCulture, ExceptionMessages.TooManyRequestsMinutes,
            (retryAfterSeconds + 59) / 60),
        _ => ExceptionMessages.DailyLimitReached
    };

    // the limiter runs before MediatR, so LocalizationBehavior has not set the culture yet
    private static string Message(HttpContext httpContext, int retryAfterSeconds)
    {
        // same order as LocalizationBehavior: Accept-Language, then the configured default
        var language = SupportedLanguages.FromAcceptLanguage(httpContext.Request.Headers.AcceptLanguage.ToString())
                       ?? httpContext.RequestServices.GetService<IOptions<LocalizationOptions>>()?.Value.DefaultLanguage;
        var culture = SupportedLanguages.IsSupported(language)
            ? CultureInfo.GetCultureInfo(SupportedLanguages.ToCulture(language!))
            : CultureInfo.InvariantCulture;

        var previousUi = CultureInfo.CurrentUICulture;
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            return TooManyRequestsText(retryAfterSeconds);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousUi;
            CultureInfo.CurrentCulture = previous;
        }
    }
}
