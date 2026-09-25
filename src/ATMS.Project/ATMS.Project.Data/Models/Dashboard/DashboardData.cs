namespace ATMS.Project.Data.Models.Dashboard;

public sealed class DashboardData
{
    public Dictionary<int, DashboardStatusCount> StatusCounts { get; init; }

    public Dictionary<int, int> PriorityCounts { get; init; }

    public int Done { get; init; }

    public int PreviousDone { get; init; }

    public Dictionary<DateTime, int> CreatedByDay { get; init; }

    public Dictionary<DateTime, int> DoneByDay { get; init; }

    public DashboardWorkloadRow[] Workload { get; init; }

    public DashboardEntityRow[] Secondary { get; init; }

    public DashboardDeadlineRow[] Deadlines { get; init; }

    public DashboardActivityRow[] Activities { get; init; }
}
