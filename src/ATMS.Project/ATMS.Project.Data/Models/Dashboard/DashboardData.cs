namespace ATMS.Project.Data.Models.Dashboard;

public sealed class DashboardData
{
    public Dictionary<int, DashboardStatusCount> StatusCounts { get; init; }

    public Dictionary<int, int> PriorityCounts { get; init; }

    public int Created { get; init; }

    public int PreviousCreated { get; init; }

    public int Done { get; init; }

    public int PreviousDone { get; init; }

    public Dictionary<DateTime, int> CreatedByBucket { get; init; }

    public Dictionary<DateTime, int> StartedByBucket { get; init; }

    public Dictionary<DateTime, int> DoneByBucket { get; init; }

    public DashboardWorkloadRow[] Workload { get; init; }

    public DashboardEntityRow[] Secondary { get; init; }

    public DashboardDeadlineRow[] Deadlines { get; init; }

    public int DeadlineCount { get; init; }

    public DashboardActivityRow[] Activities { get; init; }
}
