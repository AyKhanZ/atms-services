using MediatR;

namespace ATMS.Project.Contracts.Commands.Organization;

public class UpdateOrganizationCommand : OrganizationCommand, IRequest
{
    public required Guid Id { get; set; }
}
