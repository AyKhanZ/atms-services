using ATMS.Admin.Contracts.Commands;
using ATMS.Admin.Data.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Roles;

public class DeleteRoleHandler (
    IRoleRepository roleRepository
    ) : IRequestHandler<DeleteRoleCommand, Guid>
{
    public async Task<Guid> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        await roleRepository.DeleteAsync(command.Id, cancellationToken);
        return command.Id;
    }
}
