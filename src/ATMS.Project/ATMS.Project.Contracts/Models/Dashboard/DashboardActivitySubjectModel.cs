namespace ATMS.Project.Contracts.Models.Dashboard;

public sealed class DashboardActivitySubjectModel
{
    public required string Type { get; init; }

    public required string Code { get; init; }

    public required string Title { get; init; }

    public bool IsDeleted { get; init; }

    public bool IsSubtask { get; init; }
}
