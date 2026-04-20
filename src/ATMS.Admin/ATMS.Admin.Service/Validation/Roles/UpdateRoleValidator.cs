using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Exceptions.Resources;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Roles;

public class UpdateRoleValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleValidator(IRoleRepository roleRepository, IPermissionRepository permissionRepository)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);
        
        RuleFor(x => x)
            .SetValidator(new RoleValidator(roleRepository, permissionRepository));
    }
}