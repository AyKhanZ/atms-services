using MediatR;

namespace ATMS.Project.Contracts.Commands.Organization;

public class UpdateOrganizationCommand : OrganizationCommand, IRequest
{
    public Guid Id { get; set; }
}
