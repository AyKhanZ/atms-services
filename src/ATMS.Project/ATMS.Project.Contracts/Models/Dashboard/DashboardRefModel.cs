namespace ATMS.Project.Contracts.Models.Dashboard;

public sealed class DashboardRefModel
{
    public Guid ProjectId { get; init; }

    public Guid? WorkTicketId { get; init; }

    public Guid? WorkTaskId { get; init; }
}
