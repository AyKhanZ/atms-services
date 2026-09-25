namespace ATMS.Project.Contracts.Models.Dashboard;

public sealed class DashboardModel
{
    public DateTime GeneratedAt { get; init; }

    public string Period { get; init; }

    public DateOnly From { get; init; }

    public DateOnly To { get; init; }

    public string Granularity { get; init; }

    public DashboardKpiModel[] Kpis { get; init; }

    public DashboardSeriesChartModel MainChart { get; init; }

    public DashboardDonutChartModel[] Donuts { get; init; }

    public DashboardWorkloadModel? Workload { get; init; }

    public DashboardSecondaryChartModel SecondaryChart { get; init; }

    public DashboardDeadlineModel[] Deadlines { get; init; }

    public int DeadlineCount { get; init; }

    public DashboardActivityModel[] Activities { get; init; }
}
