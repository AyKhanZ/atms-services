using ATMS.Admin.Contracts.Commands.Authentication;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Authentication;

public class LoginValidator : AbstractValidator<LoginCommand>
{

    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required .");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required .");
    }
}
