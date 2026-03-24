using ATMS.Admin.Contracts.Commands.Authentication;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Authentication;

public class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("RefreshToken is required .");
    }
}