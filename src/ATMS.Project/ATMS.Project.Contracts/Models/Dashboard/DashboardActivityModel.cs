using ATMS.Project.Contracts.Models.History;
using ATMS.Project.Contracts.Models.WorkItems;

namespace ATMS.Project.Contracts.Models.Dashboard;

public sealed class DashboardActivityModel
{
    public WorkItemRefModel Ref { get; init; }

    public DashboardActivitySubjectModel Subject { get; init; }

    public HistoryEntryModel Entry { get; init; }
}

