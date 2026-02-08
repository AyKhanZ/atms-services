using ATMS.Admin.Contracts.Commands;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Role;

public class RoleValidator : AbstractValidator<RoleCommand>
{
    public RoleValidator()
    {
        RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Role name is required.")
            .MaximumLength(20)
            .WithMessage("Role name must not exceed 20 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(20)
            .WithMessage("Role name must not exceed 20 characters.");
    }
}
