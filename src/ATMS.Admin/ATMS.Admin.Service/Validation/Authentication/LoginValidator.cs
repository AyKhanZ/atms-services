using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Resources;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Authentication;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{

    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationMessages.EmailRequired);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(AccountMessages.PasswordRequired);
    }
}