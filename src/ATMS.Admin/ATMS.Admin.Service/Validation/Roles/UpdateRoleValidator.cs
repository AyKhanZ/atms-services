using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Roles;

public class UpdateRoleValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleValidator(IRoleRepository roleRepository, IPermissionRepository permissionRepository)
    {
        RuleFor(x => x).SetValidator(new RoleValidator(roleRepository, permissionRepository));
    }
}
