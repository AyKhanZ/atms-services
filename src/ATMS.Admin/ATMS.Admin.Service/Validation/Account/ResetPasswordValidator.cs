using ATMS.Admin.Contracts.Commands.Account;
using FluentValidation;
using ATMS.Application.Dispatcher.Validation;
using ATMS.Admin.Service.Resources;

namespace ATMS.Admin.Service.Validation.Account;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.PasswordRequired)
            .MinimumLength(6).WithMessage(string.Format(AccountMessages.PasswordTooShort, 6))
            .MaximumLength(40).WithMessage(string.Format(AccountMessages.PasswordTooLong, 40))
            .Must(password => PasswordHelper.IsValid(password, 6, false))
            .WithMessage(AccountMessages.PasswordInvalidFormat);

        RuleFor(x => x.ConfirmPassword).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.ConfirmPasswordRequired)
            .Equal(x => x.Password).WithMessage(AccountMessages.PasswordsNotMatches);

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(AccountMessages.TokenRequired);
    }
}
