namespace ATMS.Project.Contracts.Models.Dashboard;

public sealed class DashboardDoneKpiModel : DashboardKpiModel
{
    public int PreviousValue { get; init; }

    public int? ChangePercent { get; init; }
}
