namespace ATMS.Project.Contracts.Models.Dashboard;

public sealed class DashboardActivitySubjectModel
{
    public string Type { get; init; }

    public string Code { get; init; }

    public string Title { get; init; }

    public bool IsDeleted { get; init; }

    public bool IsSubtask { get; init; }
}
