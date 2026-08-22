using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

public class UpdateWorkProjectCommand : WorkProjectCommand, IRequest
{
    public required Guid Id { get; set; }
}
