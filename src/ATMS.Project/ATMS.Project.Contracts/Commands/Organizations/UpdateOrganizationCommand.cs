using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;

namespace ATMS.Project.Contracts.Commands.Organizations;

[Access(PermissionEnum.OrganizationEdit)]
public class UpdateOrganizationCommand : OrganizationCommand, IRequest
{
    public required Guid Id { get; set; }
}
