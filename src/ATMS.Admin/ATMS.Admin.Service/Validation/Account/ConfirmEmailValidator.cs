using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Service.Resources;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Account;

public class ConfirmEmailValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(AccountMessages.TokenRequired);
    }
}
