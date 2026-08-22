using MediatR;

namespace ATMS.Project.Contracts.Commands.Organizations;

public class DeleteOrganizationCommand : IRequest
{
    public required Guid Id { get; init; }
}
