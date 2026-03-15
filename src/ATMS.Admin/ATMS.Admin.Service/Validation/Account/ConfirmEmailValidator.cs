using ATMS.Admin.Contracts.Commands.Account;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Account;

public class ConfirmEmailValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailValidator()
    {
        RuleFor(x => x.Token).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Token is required")
            .MaximumLength(200).WithMessage("Token must not exceed 200 characters.");
    }
}
