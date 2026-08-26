using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;

namespace ATMS.Project.Contracts.Commands.Organizations;

[Access(PermissionEnum.OrganizationEdit)]
public class CreateOrganizationCommand : OrganizationCommand, IRequest<Guid>;
