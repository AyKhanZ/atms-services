using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Roles;
using ATMS.Admin.Data.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Roles;

public class GetRoleHandler(
    IRoleRepository roleRepository,
    IMapper mapper) : IRequestHandler<GetRoleRequest, RoleModel>
{
    public async Task<RoleModel> Handle(GetRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);

        return mapper.Map<RoleModel>(role);
    }
}
