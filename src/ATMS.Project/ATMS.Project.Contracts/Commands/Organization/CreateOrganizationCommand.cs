using MediatR;

namespace ATMS.Project.Contracts.Commands.Organization;

public class CreateOrganizationCommand : OrganizationCommand, IRequest<Guid>;