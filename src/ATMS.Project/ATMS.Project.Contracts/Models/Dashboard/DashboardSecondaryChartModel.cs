namespace ATMS.Project.Contracts.Models.Dashboard;

public sealed class DashboardSecondaryChartModel
{
    public string Key { get; init; }

    public DashboardEntitySegmentModel[] Segments { get; init; }
}

