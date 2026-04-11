using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Service.Resources;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Authentication;

public class LoginValidator : AbstractValidator<LoginCommand>
{

    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(AccountMessages.EmailRequired);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(AccountMessages.PasswordRequired);
    }
}
