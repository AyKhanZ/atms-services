using ATMS.Admin.Contracts.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Roles;

public class GetRoleRequest : IRequest<RoleModel>
{
    public Guid Id { get; set; }
}
