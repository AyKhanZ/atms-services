using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;

namespace ATMS.Project.Contracts.Commands.Organizations;

[Access(PermissionEnum.OrganizationDelete)]
public class DeleteOrganizationCommand : IRequest
{
    public required Guid Id { get; init; }
}
