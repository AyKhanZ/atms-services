using MediatR;

namespace ATMS.Project.Contracts.Commands.Organizations;

public class UpdateOrganizationCommand : OrganizationCommand, IRequest
{
    public required Guid Id { get; set; }
}
