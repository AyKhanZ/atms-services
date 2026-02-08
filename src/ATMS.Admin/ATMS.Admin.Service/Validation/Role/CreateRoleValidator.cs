using ATMS.Admin.Contracts.Commands;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Role;

public class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x).SetValidator(new RoleValidator());
    }
}
