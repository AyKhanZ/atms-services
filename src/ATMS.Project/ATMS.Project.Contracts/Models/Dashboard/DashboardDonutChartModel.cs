namespace ATMS.Project.Contracts.Models.Dashboard;

public sealed class DashboardDonutChartModel
{
    public string Key { get; init; }

    public DashboardDictionarySegmentModel[] Segments { get; init; }
}

