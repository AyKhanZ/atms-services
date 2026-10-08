using ATMS.Admin.Contracts.Commands.Account;
using FluentValidation;
using ATMS.Application.Dispatcher.Validation;
using ATMS.Admin.Service.Resources;

namespace ATMS.Admin.Service.Validation.Account;

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.PasswordRequired)
            .MinimumLength(10).WithMessage(string.Format(AccountMessages.PasswordTooShort, 10))
            .MaximumLength(40).WithMessage(string.Format(AccountMessages.PasswordTooLong, 40))
            .Must(password => PasswordHelper.IsValid(password, 10, true))
            .WithMessage(AccountMessages.PasswordInvalidFormat);

        RuleFor(x => x.ConfirmPassword).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.ConfirmPasswordRequired)
            .Equal(x => x.Password).WithMessage(AccountMessages.PasswordsNotMatches);

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(AccountMessages.TokenRequired);
    }
}
