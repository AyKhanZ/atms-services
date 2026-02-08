using ATMS.Admin.Contracts.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Commands;

public class UpdateRoleCommand : RoleCommand, IRequest<RoleModel>
{
    public Guid Id { get; set; }
}
