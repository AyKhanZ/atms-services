using ATMS.Admin.Contracts.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Commands;

public class CreateRoleCommand : RoleCommand, IRequest<RoleModel>
{
}
