using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Entity;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Roles;

public class DeleteRoleHandler (
    IRoleRepository roleRepository,
    ICacheService cache
    ) : IRequestHandler<DeleteRoleCommand>
{
    public async Task Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        var isExist = await roleRepository.IsExistAsync(r => r.Id == command.Id, cancellationToken);

        if (!isExist)
        {
            throw new EntityException(EntityErrorType.NotFound, RoleMessages.NotFound);
        }

        await roleRepository.DeleteAsync(command.Id, cancellationToken);
        
        await InvalidateRoleCacheAsync(command, cancellationToken);
    }

    private async Task InvalidateRoleCacheAsync(DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(CacheKeys.Admin.RoleById(command.Id), cancellationToken);
        await cache.RemoveAsync(CacheKeys.Admin.AllRoles, cancellationToken);
    }
}