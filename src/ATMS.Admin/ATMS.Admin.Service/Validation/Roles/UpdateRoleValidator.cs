using ATMS.Admin.Contracts.Commands.Role;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Role;

public class UpdateRoleValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x).SetValidator(new RoleValidator());
    }
}
