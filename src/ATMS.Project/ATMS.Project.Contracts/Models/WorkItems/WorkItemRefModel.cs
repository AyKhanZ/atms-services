namespace ATMS.Project.Contracts.Models.WorkItems;

public sealed class WorkItemRefModel
{
    public Guid ProjectId { get; init; }

    public Guid? WorkTicketId { get; init; }

    public Guid? WorkTaskId { get; init; }
}
