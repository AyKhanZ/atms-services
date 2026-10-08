using ATMS.Project.Contracts.Requests.Dashboard;
using ATMS.Project.Data.Enums;
using ATMS.Project.Data.Models.Dashboard;
using ATMS.Project.Services.Resources;
using FluentValidation;
using FluentValidation.Results;

using ATMS.Project.Services.Models.Dashboard;

namespace ATMS.Project.Services.Infrastructure;

public sealed class BusinessTimeZone(TimeZoneInfo zone)
{
    // longer ranges go by month, a year of daily points is just a block
    private const int MaxDailyDays = 92;
    private const int MaxRangeDays = 366;

    public DashboardPeriodWindow GetWindow(DateTime utcNow, string? period, DateOnly? from, DateOnly? to)
    {
        var today = Today(utcNow);
        var (name, firstDay, lastDay) = (period ?? DashboardPeriods.Last30Days) switch
        {
            DashboardPeriods.Today => (DashboardPeriods.Today, today, today),
            DashboardPeriods.Last7Days => (DashboardPeriods.Last7Days, today.AddDays(-6), today),
            DashboardPeriods.Last30Days => (DashboardPeriods.Last30Days, today.AddDays(-29), today),
            DashboardPeriods.ThisMonth => (DashboardPeriods.ThisMonth, new DateOnly(today.Year, today.Month, 1), today),
            DashboardPeriods.Last6Months => (DashboardPeriods.Last6Months, today.AddMonths(-6).AddDays(1), today),
            DashboardPeriods.Last12Months => (DashboardPeriods.Last12Months, today.AddYears(-1).AddDays(1), today),
            DashboardPeriods.Custom => (DashboardPeriods.Custom, CheckedStart(from, to, today), to!.Value),
            _ => throw Invalid(nameof(period), DashboardMessages.PeriodUnsupported)
        };

        var days = lastDay.DayNumber - firstDay.DayNumber + 1;
        var granularity = days == 1
            ? DashboardGranularityEnum.Hour
            : days <= MaxDailyDays ? DashboardGranularityEnum.Day : DashboardGranularityEnum.Month;

        return new DashboardPeriodWindow(
            utcNow,
            name,
            firstDay,
            lastDay,
            new DashboardDataWindow(
                ToUtc(today),
                ToUtc(firstDay),
                ToUtc(firstDay.AddDays(-days)),
                ToUtc(lastDay.AddDays(1)),
                ToUtc(today.AddDays(7)),
                zone.GetUtcOffset(utcNow).TotalHours,
                granularity));
    }

    private static DateOnly CheckedStart(DateOnly? from, DateOnly? to, DateOnly today)
    {
        if (from is not { } start || to is not { } end)
        {
            throw Invalid(nameof(from), DashboardMessages.RangeRequired);
        }

        if (start > end)
        {
            throw Invalid(nameof(from), DashboardMessages.RangeReversed);
        }

        if (end > today)
        {
            throw Invalid(nameof(to), DashboardMessages.RangeInFuture);
        }

        if (end.DayNumber - start.DayNumber + 1 > MaxRangeDays)
        {
            throw Invalid(nameof(from), DashboardMessages.RangeTooLong);
        }

        return start;
    }

    private static ValidationException Invalid(string field, string message) =>
        new([new ValidationFailure(char.ToUpperInvariant(field[0]) + field[1..], message)]);

    public DateOnly Today(DateTime utcNow) => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcNow, zone));

    // the browser sends a deadline as its own midnight, so the local day is the date
    public DateOnly DateOf(DateTime utc) => Today(utc);

    public DateTime StartOfDayUtc(DateOnly date) => ToUtc(date);

    public bool HasReached(DateTime utcNow, TimeOnly time) =>
        TimeOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcNow, zone)) >= time;

    public DateTime NextUtc(DateTime utcNow, TimeOnly time)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(utcNow, zone);
        var next = DateOnly.FromDateTime(local).ToDateTime(time, DateTimeKind.Unspecified);
        if (next <= local)
        {
            next = next.AddDays(1);
        }

        return TimeZoneInfo.ConvertTimeToUtc(next, zone);
    }

    private DateTime ToUtc(DateOnly date) => TimeZoneInfo.ConvertTimeToUtc(
        date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), zone);
}
