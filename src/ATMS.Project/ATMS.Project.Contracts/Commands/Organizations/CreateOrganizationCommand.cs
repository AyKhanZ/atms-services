using MediatR;

namespace ATMS.Project.Contracts.Commands.Organizations;

public class CreateOrganizationCommand : OrganizationCommand, IRequest<Guid>;