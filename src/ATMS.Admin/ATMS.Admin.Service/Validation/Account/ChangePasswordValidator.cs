using System.Text.RegularExpressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Resources;
using FluentValidation;
using ATMS.Application.Dispatcher.Validation;

namespace ATMS.Admin.Service.Validation.Account;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.EmailRequired)
            .EmailAddress().WithMessage(ValidationMessages.InvalidEmailFormat);
        
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage(AccountMessages.OldPasswordRequired);

        RuleFor(x => x.NewPassword).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.NewPasswordRequired)
            .MinimumLength(6).WithMessage(string.Format(AccountMessages.PasswordTooShort, 6))
            .MaximumLength(40).WithMessage(string.Format(AccountMessages.PasswordTooLong, 40))
            .Must(password => PasswordHelper.IsValid(password, 6, false))
            .WithMessage(AccountMessages.PasswordInvalidFormat);
    }

}
