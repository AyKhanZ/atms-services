using ATMS.Project.Contracts.Models.History;

namespace ATMS.Project.Contracts.Models.Dashboard;

public sealed class DashboardActivityModel
{
    public DashboardRefModel Ref { get; init; }

    public required DashboardActivitySubjectModel Subject { get; init; }

    public HistoryEntryModel Entry { get; init; }
}

