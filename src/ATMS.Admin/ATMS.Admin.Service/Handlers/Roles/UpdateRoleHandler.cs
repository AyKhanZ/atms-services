using ATMS.Admin.Contracts.Commands;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Roles;

public class UpdateRoleHandler(
    IRoleRepository roleRepository,
    IMapper mapper) : IRequestHandler<UpdateRoleCommand, RoleModel>
{
    public async Task<RoleModel> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Role>(command);

        await roleRepository.UpdateAsync(entity, cancellationToken);

        return mapper.Map<RoleModel>(entity);
    }
}
