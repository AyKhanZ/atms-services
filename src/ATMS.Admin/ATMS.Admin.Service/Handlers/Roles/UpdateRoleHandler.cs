using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Exceptions.Entity;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Roles;

public class UpdateRoleHandler(
    IRoleRepository roleRepository,
    IMapper mapper) : IRequestHandler<UpdateRoleCommand>
{
    public async Task Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var isExist = await roleRepository.IsExistAsync(r => r.Id == command.Id, cancellationToken);

        if (!isExist)
        {
            throw new EntityException(EntityErrorType.NotFound, "Role not found .");
        }

        var entity = mapper.Map<Role>(command);

        await roleRepository.UpdateAsync(entity, cancellationToken);
    }
}
