using MediatR;

namespace ATMS.Admin.Contracts.Commands.Role;

public class UpdateRoleCommand : RoleCommand, IRequest
{
    public Guid Id { get; set; }
}
