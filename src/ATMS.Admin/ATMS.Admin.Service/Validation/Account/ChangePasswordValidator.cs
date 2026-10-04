using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Dispatcher.Validation;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Account;

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage(AccountMessages.OldPasswordRequired);

        RuleFor(x => x.NewPassword).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.NewPasswordRequired)
            .MinimumLength(10).WithMessage(string.Format(AccountMessages.PasswordTooShort, 10))
            .MaximumLength(40).WithMessage(string.Format(AccountMessages.PasswordTooLong, 40))
            .Must(password => PasswordHelper.IsValid(password, 10, true))
            .WithMessage(AccountMessages.PasswordInvalidFormat)
            .NotEqual(x => x.OldPassword).WithMessage(AccountMessages.NewPasswordMustDiffer);

        RuleFor(x => x.ConfirmPassword).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.ConfirmPasswordRequired)
            .Equal(x => x.NewPassword).WithMessage(AccountMessages.PasswordsNotMatches);
    }
}
