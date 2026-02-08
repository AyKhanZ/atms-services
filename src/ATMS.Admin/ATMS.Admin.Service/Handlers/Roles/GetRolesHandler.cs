using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Roles;
using ATMS.Admin.Data.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Roles;

public class GetRolesHandler(
    IRoleRepository roleRepository,
    IMapper mapper) : IRequestHandler<GetRolesRequest, RoleModel[]>
{
    public async Task<RoleModel[]> Handle(GetRolesRequest request, CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAsync(request.UserId, cancellationToken);

        return mapper.Map<RoleModel[]>(roles);
    }
}
