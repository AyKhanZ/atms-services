using System.Text.RegularExpressions;
using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Service.Resources;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Onboarding;

public sealed class SaveSecurityValidator : AbstractValidator<SaveSecurityCommand>
{
    private static readonly Regex PasswordPattern = new(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[!@#$%^&*()\-_+=])[A-Za-z\d!@#$%^&*()\-_+=]{10,40}$", RegexOptions.Compiled);

    public SaveSecurityValidator()
    {
        RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.PasswordRequired)
            .MinimumLength(10).WithMessage(AccountMessages.PasswordTooShort)
            .MaximumLength(40).WithMessage(AccountMessages.PasswordTooLong)
            .Must(password => PasswordPattern.IsMatch(password)).WithMessage(AccountMessages.PasswordInvalidFormat);
        
        RuleFor(x => x.ConfirmPassword).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.ConfirmPasswordRequired)
            .Equal(x => x.Password).WithMessage(AccountMessages.PasswordsNotMatches);
        
        RuleFor(x => x.Version)
            .GreaterThanOrEqualTo(0).WithMessage(OnboardingMessages.VersionInvalid);
    }
}
