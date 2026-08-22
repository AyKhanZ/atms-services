using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

public class AddWorkProjectParticipantCommand : WorkProjectParticipantCommand, IRequest
{
    public Guid ProjectId { get; set; }
}
