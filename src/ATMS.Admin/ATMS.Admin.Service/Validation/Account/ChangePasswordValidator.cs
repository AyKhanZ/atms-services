using System.Text.RegularExpressions;
using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Resources;
using FluentValidation;

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
            .Must(IsValidPassword).WithMessage(AccountMessages.PasswordInvalidFormat);
    }

    private bool IsValidPassword(string password)
    {
        // Explains:
        // ^                  - start
        // (?=.*[A-Z])        - at least one Uppercase letter
        // (?=.*\d)           - at least one number
        // (?=.*[!@#$%^&*()\-_+=]) - at least one special symbol
        // [A-Za-z\d!@#$%^&*()\-_+=] {6,40} - only valid symbols, length 6-40
        // $                  - end
        var regex = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()\-_+=])[A-Za-z\d!@#$%^&*()\-_+=]{6,40}$");
        return regex.IsMatch(password);
    }
}