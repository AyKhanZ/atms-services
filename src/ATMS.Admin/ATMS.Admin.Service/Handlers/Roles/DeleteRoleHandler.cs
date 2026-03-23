using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Exceptions.Entity;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Roles;

public class DeleteRoleHandler (
    IRoleRepository roleRepository
    ) : IRequestHandler<DeleteRoleCommand>
{
    public async Task Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        var isExist = await roleRepository.IsExistAsync(r => r.Id == command.Id, cancellationToken);

        if (!isExist)
        {
            throw new EntityException(EntityErrorType.NotFound, "Role not found .");
        }

        await roleRepository.DeleteAsync(command.Id, cancellationToken);
    }
}
