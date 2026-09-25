using ATMS.Application.Exceptions.Configuration;
using ATMS.Project.Services.Dashboard;
using ATMS.Project.Services.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Project.Services.Tests.Dashboard;

public sealed class BusinessTimeZoneTest
{
    [Theory]
    [InlineData(7, "2026-09-19")]
    [InlineData(30, "2026-08-27")]
    [InlineData(90, "2026-06-28")]
    public void GetWindow_UsesBakuCalendarDays(int period, string firstDay)
    {
        var zone = new BusinessTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku"));

        var window = zone.GetWindow(new DateTime(2026, 9, 24, 21, 30, 0, DateTimeKind.Utc), period);

        Assert.Equal(DateOnly.Parse(firstDay), window.FirstDay);
        Assert.Equal(new DateTime(2026, 9, 24, 20, 0, 0, DateTimeKind.Utc), window.TodayStartUtc);
        Assert.Equal(new DateTime(2026, 9, 25, 20, 0, 0, DateTimeKind.Utc), window.PeriodEndUtc);
        Assert.Equal(new DateTime(2026, 10, 1, 20, 0, 0, DateTimeKind.Utc), window.DueEndUtc);
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
