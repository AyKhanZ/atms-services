using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Roles;

public class DeleteRoleHandler (
    IRoleRepository roleRepository
    ) : IRequestHandler<DeleteRoleCommand>
{
    public async Task Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        await roleRepository.DeleteAsync(command.Id, cancellationToken);
    }
}
