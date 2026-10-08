
using ATMS.Project.Data.Enums;

namespace ATMS.Project.Data.Models.Dashboard;

public sealed record DashboardDataWindow(
    DateTime TodayStartUtc,
    DateTime PeriodStartUtc,
    DateTime PreviousStartUtc,
    DateTime PeriodEndUtc,
    DateTime DueEndUtc,
    double OffsetHours,
    DashboardGranularityEnum Granularity);
