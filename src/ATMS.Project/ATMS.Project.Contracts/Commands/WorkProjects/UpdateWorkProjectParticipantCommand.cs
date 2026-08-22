using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

public class UpdateWorkProjectParticipantCommand : IRequest
{
    public Guid ProjectId { get; set; }

    public Guid ParticipantId { get; set; }

    public Guid RoleId { get; set; }
}
