using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Roles;

public class CreateRoleHandler(
    IMapper mapper,
    IRoleRepository roleRepository)
    : IRequestHandler<CreateRoleCommand, RoleModel>
{
    public async Task<RoleModel> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<Role>(command);
        entity.Id = Guid.NewGuid();

        await roleRepository.CreateAsync(entity, cancellationToken);

        return mapper.Map<RoleModel>(entity);
    }
}
