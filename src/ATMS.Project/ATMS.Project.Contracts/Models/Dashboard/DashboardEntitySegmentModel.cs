namespace ATMS.Project.Contracts.Models.Dashboard;

public sealed class DashboardEntitySegmentModel
{
    public Guid Id { get; init; }

    public string Code { get; init; }

    public string Label { get; init; }

    public int Value { get; init; }
}

