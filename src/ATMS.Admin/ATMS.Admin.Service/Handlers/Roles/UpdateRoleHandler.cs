using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Entity;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Roles;

public class UpdateRoleHandler(
    IRoleRepository roleRepository,
    ICacheService cache) : IRequestHandler<UpdateRoleCommand>
{
    public async Task Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await roleRepository.FindAsync(r => r.Id == command.Id, cancellationToken);

        if (role is null)
        {
            throw new EntityException(EntityErrorType.NotFound, RoleMessages.NotFound);
        }

        var newPermissions = command.PermissionIds.Distinct().ToList();
        var toRemove = role.RolePermissions.Where(rp => !newPermissions.Contains(rp.PermissionId)).ToList();
        var toAdd = newPermissions
            .Where(id => role.RolePermissions.All(rp => rp.PermissionId != id))
            .Select(id => new RolePermission { RoleId = role.Id, PermissionId = id })
            .ToList();

        role.Name = command.Name;
        role.Description = command.Description;
        role.RolePermissions.RemoveAll(rp => toRemove.Contains(rp));
        role.RolePermissions.AddRange(toAdd);

        await roleRepository.SaveAsync(cancellationToken);
        
        await InvalidateRoleCacheAsync(command, cancellationToken);
    }

    private async Task InvalidateRoleCacheAsync(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(CacheKeys.Admin.RoleById(command.Id), cancellationToken);
        await cache.RemoveAsync(CacheKeys.Admin.AllRoles, cancellationToken);
    }
}