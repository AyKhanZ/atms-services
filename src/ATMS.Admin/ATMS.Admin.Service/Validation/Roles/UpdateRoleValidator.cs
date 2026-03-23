using ATMS.Admin.Contracts.Commands.Role;
using ATMS.Admin.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Roles;

public class UpdateRoleValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleValidator(IRoleRepository roleRepository)
    {
        RuleFor(x => x).SetValidator(new RoleValidator(roleRepository));
    }
}
