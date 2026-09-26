using ATMS.Application.Exceptions.Configuration;
using ATMS.Project.Data.Models.Dashboard;
using ATMS.Project.Services.Dashboard;
using ATMS.Project.Services.Modules;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Project.Services.Tests.Dashboard;

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
    public void AddDashboardServices_RejectsMissingOrUnknownTimeZone(string? id)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["BusinessTimeZone"] = id })
            .Build();

        var exception = Assert.Throws<ConfigurationException>(() =>
            new ServiceCollection().AddDashboardServices(configuration));
        Assert.Equal(
            id is null or "" ? ConfigurationErrorType.BusinessTimeZoneNotFound
                : ConfigurationErrorType.BusinessTimeZoneUnavailable,
            exception.ErrorType);
    }
}
