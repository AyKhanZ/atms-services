using ATMS.Application.Exceptions.Configuration;
using ATMS.Project.Data.Models.Dashboard;
using ATMS.Project.Services.Dashboard;
using ATMS.Project.Services.Time;
using ATMS.Project.Services.Modules;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Project.Services.Tests.Time;

public sealed class BusinessTimeZoneTest
{
    // 21:30 UTC on 24 September is already 01:30 on 25 September in Baku.
    private static readonly DateTime UtcNow = new(2026, 9, 24, 21, 30, 0, DateTimeKind.Utc);
    private readonly BusinessTimeZone _zone = new(TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku"));

    [Theory]
    [InlineData("today", "2026-09-25", DashboardGranularity.Hour)]
    [InlineData("7d", "2026-09-19", DashboardGranularity.Day)]
    [InlineData("30d", "2026-08-27", DashboardGranularity.Day)]
    [InlineData("thisMonth", "2026-09-01", DashboardGranularity.Day)]
    [InlineData("6m", "2026-03-26", DashboardGranularity.Month)]
    [InlineData("12m", "2025-09-26", DashboardGranularity.Month)]
    public void GetWindow_NamedPeriod_EndsTodayInBaku(string period, string firstDay, DashboardGranularity granularity)
    {
        var window = _zone.GetWindow(UtcNow, period, null, null);

        Assert.Equal(period, window.Period);
        Assert.Equal(DateOnly.Parse(firstDay), window.FirstDay);
        Assert.Equal(new DateOnly(2026, 9, 25), window.LastDay);
        Assert.Equal(granularity, window.Data.Granularity);
        Assert.Equal(new DateTime(2026, 9, 24, 20, 0, 0, DateTimeKind.Utc), window.Data.TodayStartUtc);
        Assert.Equal(new DateTime(2026, 9, 25, 20, 0, 0, DateTimeKind.Utc), window.Data.PeriodEndUtc);
        Assert.Equal(new DateTime(2026, 10, 1, 20, 0, 0, DateTimeKind.Utc), window.Data.DueEndUtc);
    }

    [Fact]
    public void GetWindow_NoPeriod_UsesLast30Days()
    {
        var window = _zone.GetWindow(UtcNow, null, null, null);

        Assert.Equal("30d", window.Period);
        Assert.Equal(new DateOnly(2026, 8, 27), window.FirstDay);
    }

    [Fact]
    public void GetWindow_Custom_ComparesWithTheSameLengthBefore()
    {
        var window = _zone.GetWindow(UtcNow, "custom", new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 10));

        Assert.Equal(new DateOnly(2026, 9, 1), window.FirstDay);
        Assert.Equal(new DateOnly(2026, 9, 10), window.LastDay);
        Assert.Equal(DashboardGranularity.Day, window.Data.Granularity);
        Assert.Equal(new DateTime(2026, 8, 31, 20, 0, 0, DateTimeKind.Utc), window.Data.PeriodStartUtc);
        Assert.Equal(new DateTime(2026, 8, 21, 20, 0, 0, DateTimeKind.Utc), window.Data.PreviousStartUtc);
        Assert.Equal(new DateTime(2026, 9, 10, 20, 0, 0, DateTimeKind.Utc), window.Data.PeriodEndUtc);
    }

    [Fact]
    public void GetWindow_CustomSingleDay_IsDrawnByHour()
    {
        var day = new DateOnly(2026, 9, 3);

        var window = _zone.GetWindow(UtcNow, "custom", day, day);

        Assert.Equal(DashboardGranularity.Hour, window.Data.Granularity);
    }

    [Theory]
    [InlineData(null, "2026-09-10", "From")]
    [InlineData("2026-09-10", null, "From")]
    [InlineData("2026-09-10", "2026-09-01", "From")]
    [InlineData("2026-09-01", "2026-09-26", "To")]
    [InlineData("2025-09-01", "2026-09-25", "From")]
    public void GetWindow_InvalidCustomRange_ReturnsValidationError(string? from, string? to, string field)
    {
        var exception = Assert.Throws<ValidationException>(() => _zone.GetWindow(
            UtcNow,
            "custom",
            from is null ? null : DateOnly.Parse(from),
            to is null ? null : DateOnly.Parse(to)));

        Assert.Equal(field, Assert.Single(exception.Errors).PropertyName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Unknown/Zone")]
    public void AddTimeServices_RejectsMissingOrUnknownTimeZone(string? id)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["BusinessTimeZone"] = id })
            .Build();

        var exception = Assert.Throws<ConfigurationException>(() =>
            new ServiceCollection().AddTimeServices(configuration));
        Assert.Equal(
            id is null or "" ? ConfigurationErrorType.BusinessTimeZoneNotFound
                : ConfigurationErrorType.BusinessTimeZoneUnavailable,
            exception.ErrorType);
    }

    [Fact]
    public void Today_IsTheDayInBaku()
    {
        Assert.Equal(new DateOnly(2026, 9, 25), _zone.Today(UtcNow));
    }

    [Theory]
    // A deadline of 6 October picked in Baku: the browser sends its own midnight.
    [InlineData("2026-10-05T20:00:00Z", "2026-10-06")]
    // The same date picked in London: midnight there is 04:00 in Baku, the same day.
    [InlineData("2026-10-05T23:00:00Z", "2026-10-06")]
    public void DateOf_ReadsADeadlineAsTheDateInBaku(string stored, string date)
    {
        var utc = DateTime.Parse(stored, null, System.Globalization.DateTimeStyles.AdjustToUniversal);

        Assert.Equal(DateOnly.Parse(date), _zone.DateOf(utc));
    }

    [Fact]
    public void StartOfDayUtc_IsMidnightInBaku()
    {
        Assert.Equal(
            new DateTime(2026, 10, 5, 20, 0, 0, DateTimeKind.Utc),
            _zone.StartOfDayUtc(new DateOnly(2026, 10, 6)));
    }

    [Theory]
    // 08:59 in Baku: 09:00 is still ahead today.
    [InlineData("2026-10-06T04:59:00Z", "2026-10-06T05:00:00Z", false)]
    // 09:00 sharp: the pass is due now, the next one is tomorrow.
    [InlineData("2026-10-06T05:00:00Z", "2026-10-07T05:00:00Z", true)]
    // 18:00 in Baku: tomorrow morning.
    [InlineData("2026-10-06T14:00:00Z", "2026-10-07T05:00:00Z", true)]
    public void NextUtc_IsTheNextNineOClockInBaku(string now, string next, bool reached)
    {
        var utcNow = DateTime.Parse(now, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
        var nine = new TimeOnly(9, 0);

        Assert.Equal(
            DateTime.Parse(next, null, System.Globalization.DateTimeStyles.AdjustToUniversal),
            _zone.NextUtc(utcNow, nine));
        Assert.Equal(reached, _zone.HasReached(utcNow, nine));
    }
}
