using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Service.Resources;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Authentication;

public sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(AccountMessages.TokenRequired);
    }
}
