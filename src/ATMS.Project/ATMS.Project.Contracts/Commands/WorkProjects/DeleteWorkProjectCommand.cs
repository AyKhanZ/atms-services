using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

public class DeleteWorkProjectCommand : IRequest
{
    public required Guid Id { get; set; }
}
