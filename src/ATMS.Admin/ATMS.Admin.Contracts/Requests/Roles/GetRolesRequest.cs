using ATMS.Admin.Contracts.Models;
using MediatR;

namespace ATMS.Admin.Contracts.Requests.Roles;

public class GetRolesRequest : IRequest<RoleModel[]>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
