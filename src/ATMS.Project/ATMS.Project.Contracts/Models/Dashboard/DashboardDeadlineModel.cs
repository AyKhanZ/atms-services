using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Models.WorkItems;

namespace ATMS.Project.Contracts.Models.Dashboard;

public sealed class DashboardDeadlineModel
{
    public WorkItemRefModel Ref { get; init; }

    public string Code { get; init; }

    public string Title { get; init; }

    public bool IsSubtask { get; init; }

    public DateTime Deadline { get; init; }

    public DashboardPriorityModel Priority { get; init; }

    public PersonModel? Assignee { get; init; }
}

