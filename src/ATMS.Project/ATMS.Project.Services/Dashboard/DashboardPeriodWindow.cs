namespace ATMS.Project.Services.Dashboard;

public sealed record DashboardPeriodWindow(
    DateTime GeneratedAt,
    DateTime TodayStartUtc,
    DateTime PeriodStartUtc,
    DateTime PreviousStartUtc,
    DateTime PeriodEndUtc,
    DateTime DueEndUtc,
    double OffsetHours,
    DateOnly FirstDay);
