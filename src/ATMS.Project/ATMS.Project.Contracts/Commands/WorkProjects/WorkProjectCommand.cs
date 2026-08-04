namespace ATMS.Project.Contracts.Commands.WorkProjects;

public class WorkProjectCommand
{
    public required string Title { get; set; }

    public string? Description { get; set; }

    public Guid? OrganizationId { get; set; }

    public required int ProjectTypeId { get; set; }

    public required int ProjectKindId { get; set; }

    public required int ProjectStatusId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public WorkProjectParticipantCommand[] Participants { get; set; } = [];
}
