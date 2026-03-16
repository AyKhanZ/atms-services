using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Roles;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Exceptions.Entity;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Roles;

public class GetRoleHandler(
    IRoleRepository roleRepository,
    IMapper mapper) : IRequestHandler<GetRoleRequest, RoleModel>
{
    public async Task<RoleModel> Handle(GetRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetAsync(r => r.Id == request.Id, cancellationToken);

        if (role is null)
        {
            throw new EntityException(EntityErrorType.NotFound, "Role not found");
        }

        return mapper.Map<RoleModel>(role);
    }
}
