using ATMS.Project.Contracts.Models.History;

namespace ATMS.Project.Contracts.Models.Dashboard;

public sealed class DashboardDeadlineModel
{
    public DashboardRefModel Ref { get; init; }

    public string Code { get; init; }

    public string Title { get; init; }

    public bool IsSubtask { get; init; }

    public DateTime Deadline { get; init; }

    public DashboardPriorityModel Priority { get; init; }

    public HistoryPersonModel? Assignee { get; init; }
}

