namespace ATMS.Project.Contracts.Models.Dashboard;

public sealed class DashboardSeriesChartModel
{
    public string[] Labels { get; init; }

    public DashboardSeriesModel[] Series { get; init; }
}

