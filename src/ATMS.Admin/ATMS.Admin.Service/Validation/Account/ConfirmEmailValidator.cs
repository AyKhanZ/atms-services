using ATMS.Admin.Contracts.Commands.Account;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Account;

public class ConfirmEmailValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required");
    }
}
