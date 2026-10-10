using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;
using ATMS.Infrastructure.Options;
using ATMS.Swagger.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace ATMS.Swagger.RateLimiting;

// turns a request into "which counter and which limit"; the counters live in process memory, one per key
internal sealed class RateLimitPartitions(RateLimitOptions limits)
{
    // read back by RateLimitRejection to log and answer the request that was turned away
    internal const string PolicyItem = "RateLimitPolicy";
    internal const string KeyItem = "RateLimitKey";
    internal const string RetryAfterItem = "RateLimitRetryAfter";

    private const string AllRequests = "all-requests";
    private const string Mutations = "default-mutations";
    private const string DailyQuota = "daily-quota";
    private const string UserPrefix = "user:";

    // the limiter keeps the first factory it sees for a key, so skipped requests must not reuse a real key
    private const string NoLimitKey = "noop";

    private static readonly TimeSpan Day = TimeSpan.FromDays(1);

    internal static string Key(HttpContext context)
    {
        var sub = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return string.IsNullOrWhiteSpace(sub) ? IpKey(context) : $"{UserPrefix}{sub}";
    }

    // login, refresh and emails before sign-in have no user yet
    internal static string IpKey(HttpContext context) => $"ip:{context.Connection.RemoteIpAddress}";

    // every request passes all three; each one returns "no limit" when it does not apply
    public PartitionedRateLimiter<HttpContext> Global() =>
        PartitionedRateLimiter.CreateChained(
            PartitionedRateLimiter.Create<HttpContext, string>(AllRequestsPartition),
            PartitionedRateLimiter.Create<HttpContext, string>(MutationsPartition),
            PartitionedRateLimiter.Create<HttpContext, string>(DailyQuotaPartition));

    public RateLimitPartition<string> TokenBucket(
        HttpContext context,
        string policy,
        string key,
        RateLimitOptions.TokenBucketLimit limit)
    {
        if (!IsActive(context))
        {
            return NoLimit();
        }

        var limiterKey = Remember(context, policy, key);
        return RateLimitPartition.GetTokenBucketLimiter(limiterKey, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = limit.TokenLimit,
            TokensPerPeriod = limit.TokensPerPeriod,
            ReplenishmentPeriod = TimeSpan.FromSeconds(limit.PeriodSeconds),
            QueueLimit = 0
        });
    }

    public RateLimitPartition<string> SlidingWindow(
        HttpContext context,
        string policy,
        string key,
        RateLimitOptions.SlidingWindowLimit limit)
    {
        if (!IsActive(context))
        {
            return NoLimit();
        }

        var window = TimeSpan.FromSeconds(limit.WindowSeconds);
        // sliding window reports zero as Retry-After, its segment is the real wait
        var limiterKey = Remember(context, policy, key, window / limit.SegmentsPerWindow);
        return RateLimitPartition.GetSlidingWindowLimiter(limiterKey, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = limit.PermitLimit,
            Window = window,
            SegmentsPerWindow = limit.SegmentsPerWindow,
            QueueLimit = 0
        });
    }

    public RateLimitPartition<string> FixedWindow(
        HttpContext context,
        string policy,
        string key,
        RateLimitOptions.FixedWindowLimit limit) =>
        FixedWindow(context, policy, key, limit.PermitLimit, TimeSpan.FromSeconds(limit.WindowSeconds));

    public RateLimitPartition<string> Concurrency(
        HttpContext context,
        string policy,
        string key,
        RateLimitOptions.ConcurrencyLimit limit)
    {
        if (!IsActive(context))
        {
            return NoLimit();
        }

        var limiterKey = Remember(context, policy, key);
        return RateLimitPartition.GetConcurrencyLimiter(limiterKey, _ => new ConcurrencyLimiterOptions
        {
            PermitLimit = limit.PermitLimit,
            // a short queue only: the client gives up after 10 s and shows "server unavailable"
            QueueLimit = limit.QueueLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    }

    private RateLimitPartition<string> AllRequestsPartition(HttpContext context)
    {
        var key = Key(context);
        var limit = key.StartsWith(UserPrefix, StringComparison.Ordinal) ? limits.UserRequests : limits.IpRequests;
        return TokenBucket(context, AllRequests, key, limit);
    }

    // a new PUT or DELETE is limited even if nobody remembers to put an attribute on it
    private RateLimitPartition<string> MutationsPartition(HttpContext context) =>
        IsMutation(context.Request.Method) && EndpointPolicy(context) is null
            ? TokenBucket(context, Mutations, Key(context), limits.Mutations)
            : NoLimit();

    private RateLimitPartition<string> DailyQuotaPartition(HttpContext context)
    {
        var policy = EndpointPolicy(context);
        var quota = policy switch
        {
            RateLimitPolicies.Creates => limits.Creates,
            RateLimitPolicies.Emails => limits.Emails,
            RateLimitPolicies.Uploads => limits.Uploads,
            _ => null
        };

        // the policy name in the key keeps creates, emails and uploads on separate days
        return quota is null
            ? NoLimit()
            : FixedWindow(context, DailyQuota, $"{policy}:{Key(context)}", quota.DailyLimit, Day);
    }

    private RateLimitPartition<string> FixedWindow(
        HttpContext context,
        string policy,
        string key,
        int permitLimit,
        TimeSpan window)
    {
        if (!IsActive(context))
        {
            return NoLimit();
        }

        var limiterKey = Remember(context, policy, key);
        return RateLimitPartition.GetFixedWindowLimiter(limiterKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            QueueLimit = 0
        });
    }

    private bool IsActive(HttpContext context) => limits.Enabled == true && !IsSkipped(context);

    private static bool IsSkipped(HttpContext context)
    {
        var path = context.Request.Path;
        return path.StartsWithSegments("/health") ||
               path.StartsWithSegments("/alive") ||
               context.GetEndpoint()?.Metadata.GetMetadata<DisableRateLimitingAttribute>() is not null;
    }

    private static bool IsMutation(string method) =>
        HttpMethods.IsPost(method) || HttpMethods.IsPut(method) ||
        HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);

    private static string? EndpointPolicy(HttpContext context) =>
        context.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName;

    private static RateLimitPartition<string> NoLimit() => RateLimitPartition.GetNoLimiter(NoLimitKey);

    // the last limiter that ran is the one that said no
    private static string Remember(HttpContext context, string policy, string key, TimeSpan? retryAfter = null)
    {
        var limiterKey = $"{policy}:{key}";
        context.Items[PolicyItem] = policy;
        context.Items[KeyItem] = limiterKey;
        context.Items[RetryAfterItem] = retryAfter;
        return limiterKey;
    }
}
