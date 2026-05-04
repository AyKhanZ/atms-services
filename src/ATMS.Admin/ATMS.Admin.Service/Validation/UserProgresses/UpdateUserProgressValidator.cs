using System.Text.RegularExpressions;
using ATMS.Admin.Contracts.Commands.UserProgresses;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Interfaces;
using ATMS.Data.Enums;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.UserProgresses;

public class UpdateUserProgressValidator : AbstractValidator<UpdateUserProgressCommand>
{
    public UpdateUserProgressValidator(
        IDictionariesRepository dictionariesRepository,
        IUserRepository userRepository,
        ICurrentUser currentUser)
    {
        RuleFor(c => c).Cascade(CascadeMode.Stop)
            .Must(_ => !currentUser.HasCompletedSurvey).WithMessage("You already completed survey.")
            .Must(_ => currentUser.EmailConfirmed).WithMessage("Your email should be confirmed.");

        RuleFor(c => c.Password).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.NewPasswordRequired)
            .MinimumLength(6).WithMessage(string.Format(AccountMessages.PasswordTooShort, 6))
            .MaximumLength(40).WithMessage(string.Format(AccountMessages.PasswordTooLong, 40))
            .Must(IsValidPassword).WithMessage(AccountMessages.PasswordInvalidFormat)
            .When(c => c.Password is not null);

        RuleFor(c => c.PersonalInfoCommand).Cascade(CascadeMode.Stop)
            .SetValidator(new PersonalInfoValidator(dictionariesRepository)!)
            .When(c => c.PersonalInfoCommand is not null);

        RuleForEach(c => c.InvitedUsersCommand)
            .SetValidator(new InvitedUsersValidator(userRepository))
            .When(c => c.InvitedUsersCommand is { Count: > 0 }
                       && Enum.Parse<UserProgressTypeEnum>(currentUser.UserType) == UserProgressTypeEnum.ClientManager);
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