namespace ATMS.Project.Contracts.Commands.WorkTasks;

public abstract class WorkTaskCommand
{
    public Guid ProjectId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int PriorityId { get; set; }
    public DateTime? Deadline { get; set; }
    public Guid? AssigneeId { get; set; }
}
