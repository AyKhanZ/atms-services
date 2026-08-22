using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

public class DeleteWorkProjectParticipantCommand : IRequest
{
    public Guid ProjectId { get; set; }

    public Guid ParticipantId { get; set; }
}
