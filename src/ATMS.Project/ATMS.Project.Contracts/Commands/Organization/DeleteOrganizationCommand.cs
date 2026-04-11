using MediatR;

namespace ATMS.Project.Contracts.Commands.Organization;

public class DeleteOrganizationCommand : IRequest
{
    public required Guid Id { get; init; }
}
