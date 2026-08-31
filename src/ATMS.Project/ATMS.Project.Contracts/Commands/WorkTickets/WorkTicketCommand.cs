namespace ATMS.Project.Contracts.Commands.WorkTickets;

public abstract class WorkTicketCommand
{
    public Guid ProjectId { get; set; }

    public required string Title { get; set; }

    public string? Description { get; set; }

    public required Guid MilestoneId { get; set; }

    public int WorkTicketTypeId { get; set; }

    public int PriorityId { get; set; }

    public DateTime? Deadline { get; set; }

    public Guid? AssigneeId { get; set; }
}
