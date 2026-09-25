namespace ATMS.Project.Services.Dashboard;

public sealed class BusinessTimeZone(TimeZoneInfo zone)
{
    public DashboardPeriodWindow GetWindow(DateTime utcNow, int days)
    {
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcNow, zone));
        var start = today.AddDays(1 - days);

        return new DashboardPeriodWindow(
            utcNow,
            ToUtc(today),
            ToUtc(start),
            ToUtc(start.AddDays(-days)),
            ToUtc(today.AddDays(1)),
            ToUtc(today.AddDays(7)),
            zone.GetUtcOffset(utcNow).TotalHours,
            start);
    }

    private DateTime ToUtc(DateOnly date) => TimeZoneInfo.ConvertTimeToUtc(
        date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), zone);
}
