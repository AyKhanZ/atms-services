using System.Globalization;
using System.Threading.RateLimiting;
using ATMS.Swagger.RateLimiting;

namespace ATMS.Swagger.Tests;

public class RateLimitRejectionTests
{
    [Fact]
    public void RetryAfterSeconds_WithoutMetadata_ReturnsOne()
    {
        Assert.Equal(1, RateLimitRejection.RetryAfterSeconds(new FixedRetryLease(null)));
    }

    [Fact]
    public void RetryAfterSeconds_RoundsUpAndKeepsAtLeastOne()
    {
        Assert.Equal(3, RateLimitRejection.RetryAfterSeconds(new FixedRetryLease(TimeSpan.FromSeconds(2.2))));
        Assert.Equal(12, RateLimitRejection.RetryAfterSeconds(new FixedRetryLease(TimeSpan.FromSeconds(12))));
        Assert.Equal(1, RateLimitRejection.RetryAfterSeconds(new FixedRetryLease(TimeSpan.Zero)));
    }

    [Fact]
    public void RetryAfterSeconds_ZeroMetadata_UsesFallback()
    {
        Assert.Equal(10, RateLimitRejection.RetryAfterSeconds(
            new FixedRetryLease(TimeSpan.Zero), TimeSpan.FromSeconds(10)));
        Assert.Equal(3, RateLimitRejection.RetryAfterSeconds(
            new FixedRetryLease(TimeSpan.FromSeconds(3)), TimeSpan.FromSeconds(10)));
    }

    [Theory]
    [InlineData(1, "Too many requests. Try again in 1 s.")]
    [InlineData(59, "Too many requests. Try again in 59 s.")]
    [InlineData(60, "Too many requests. Try again in 1 min.")]
    [InlineData(61, "Too many requests. Try again in 2 min.")]
    [InlineData(900, "Too many requests. Try again in 15 min.")]
    [InlineData(3599, "Too many requests. Try again in 60 min.")]
    [InlineData(3600, "Daily limit reached. Try again tomorrow.")]
    [InlineData(86400, "Daily limit reached. Try again tomorrow.")]
    public void TooManyRequestsText_PicksUnitByRetryAfter(int seconds, string expected)
    {
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        try
        {
            Assert.Equal(expected, RateLimitRejection.TooManyRequestsText(seconds));
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void RejectionLogThrottle_SameKeyTwice_LogsOnce()
    {
        var throttle = new RejectionLogThrottle();

        Assert.True(throttle.ShouldLog("creates", "creates:user:1"));
        Assert.False(throttle.ShouldLog("creates", "creates:user:1"));
        Assert.True(throttle.ShouldLog("creates", "creates:user:2"));
    }

    private sealed class FixedRetryLease(TimeSpan? retryAfter) : RateLimitLease
    {
        public override bool IsAcquired => false;

        public override IEnumerable<string> MetadataNames => [];

        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            if (retryAfter is TimeSpan span && metadataName == MetadataName.RetryAfter.Name)
            {
                metadata = span;
                return true;
            }

            metadata = null;
            return false;
        }
    }
}
