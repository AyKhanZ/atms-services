using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Exceptions.Entity;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Roles;

public class UpdateRoleHandler(
    IRoleRepository roleRepository) : IRequestHandler<UpdateRoleCommand>
{
    public async Task Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var role = await roleRepository.FindAsync(r => r.Id == command.Id, cancellationToken);

        if (role is null)
        {
            throw new EntityException(EntityErrorType.NotFound, "Role not found .");
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
    }
}
