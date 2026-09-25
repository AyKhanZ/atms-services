using System.Text.Json.Serialization;

namespace ATMS.Project.Contracts.Models.Dashboard;

[JsonDerivedType(typeof(DashboardTrendKpiModel))]
public class DashboardKpiModel
{
    public string Key { get; init; }

    public int Value { get; init; }
}

