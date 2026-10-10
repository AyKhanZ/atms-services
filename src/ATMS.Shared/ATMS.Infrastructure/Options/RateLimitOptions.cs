using System.ComponentModel.DataAnnotations;

namespace ATMS.Infrastructure.Options;

// numbers live in appsettings.json; a missing or zero value stops the app at start
public sealed class RateLimitOptions
{
    [Required]
    public required bool? Enabled { get; init; }

    // every request of a signed-in user / of an IP before sign-in
    [Required]
    public required TokenBucketLimit UserRequests { get; init; }

    [Required]
    public required TokenBucketLimit IpRequests { get; init; }

    // POST / PUT / PATCH / DELETE that have no policy of their own
    [Required]
    public required TokenBucketLimit Mutations { get; init; }

    [Required]
    public required SlidingWindowLimit AuthLogin { get; init; }

    [Required]
    public required SlidingWindowLimit AuthRefresh { get; init; }

    [Required]
    public required FixedWindowLimit AuthEmail { get; init; }

    [Required]
    public required FixedWindowLimit AuthToken { get; init; }

    [Required]
    public required QuotaLimit Creates { get; init; }

    [Required]
    public required QuotaLimit Emails { get; init; }

    [Required]
    public required QuotaLimit Uploads { get; init; }

    [Required]
    public required ConcurrencyLimit Downloads { get; init; }

    [Required]
    public required ConcurrencyLimit Heavy { get; init; }

    [Required]
    public required ConcurrencyLimit Migrations { get; init; }

    public class TokenBucketLimit
    {
        [Range(1, int.MaxValue)]
        public int TokenLimit { get; init; }

        [Range(1, int.MaxValue)]
        public int TokensPerPeriod { get; init; }

        [Range(1, int.MaxValue)]
        public int PeriodSeconds { get; init; }
    }

    // a token bucket plus a cap per day
    public sealed class QuotaLimit : TokenBucketLimit
    {
        [Range(1, int.MaxValue)]
        public int DailyLimit { get; init; }
    }

    public sealed class SlidingWindowLimit
    {
        [Range(1, int.MaxValue)]
        public int PermitLimit { get; init; }

        [Range(1, int.MaxValue)]
        public int WindowSeconds { get; init; }

        [Range(1, int.MaxValue)]
        public int SegmentsPerWindow { get; init; }
    }

    public sealed class FixedWindowLimit
    {
        [Range(1, int.MaxValue)]
        public int PermitLimit { get; init; }

        [Range(1, int.MaxValue)]
        public int WindowSeconds { get; init; }
    }

    public sealed class ConcurrencyLimit
    {
        [Range(1, int.MaxValue)]
        public int PermitLimit { get; init; }

        [Range(0, int.MaxValue)]
        public int QueueLimit { get; init; }
    }
}
