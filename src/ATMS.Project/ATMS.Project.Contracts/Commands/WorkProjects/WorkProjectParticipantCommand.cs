namespace ATMS.Project.Contracts.Commands.WorkProjects;

public class WorkProjectParticipantCommand
{
    public required Guid UserId { get; set; }

    public required Guid RoleId { get; set; }
}
