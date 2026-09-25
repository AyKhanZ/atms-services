using ATMS.Project.Contracts.Models.History;

namespace ATMS.Project.Contracts.Models.Dashboard;

public sealed class DashboardWorkloadSegmentModel
{
    public string Kind { get; init; }

    public HistoryPersonModel? Person { get; init; }

    public int Value { get; init; }
}

