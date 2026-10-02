namespace ATMS.Project.Contracts.Models.WorkItems;

/// <summary>Where a project, ticket or task opens: the ids its page address is built from.</summary>
public sealed class WorkItemRefModel
{
    public Guid ProjectId { get; init; }

    public Guid? WorkTicketId { get; init; }

    public Guid? WorkTaskId { get; init; }
}
