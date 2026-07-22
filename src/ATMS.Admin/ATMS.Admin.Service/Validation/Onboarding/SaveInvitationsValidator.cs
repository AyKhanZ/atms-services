using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Interfaces;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Onboarding;

public sealed class SaveInvitationsValidator : AbstractValidator<SaveInvitationsCommand>
{
    private const int MaxInvitations = 10;

    public SaveInvitationsValidator(ICurrentUser currentUser, IOnboardingRepository onboardingRepository)
    {
        RuleFor(x => x.Users)
            .NotEmpty().WithMessage(OnboardingMessages.InvitationsRequired)
            .Must(x => x.Count <= MaxInvitations).WithMessage(OnboardingMessages.InvitationLimit);

        RuleForEach(x => x.Users)
            .ChildRules(user =>
            {
                user.RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
                    .NotEmpty().WithMessage(AccountMessages.NameRequired)
                    .MaximumLength(50).WithMessage(string.Format(AccountMessages.NameShouldBeLessThan, 50));

                user.RuleFor(x => x.Surname).Cascade(CascadeMode.Stop)
                    .NotEmpty().WithMessage(AccountMessages.SurnameRequired)
                    .MaximumLength(100).WithMessage(string.Format(AccountMessages.SurnameShouldBeLessThan, 100));

                user.RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
                    .NotEmpty().WithMessage(AccountMessages.EmailRequired)
                    .EmailAddress().WithMessage(OnboardingMessages.InvalidEmail)
                    .MaximumLength(100).WithMessage(string.Format(AccountMessages.EmailShouldBeLessThan, 100));
            });

        RuleFor(x => x.Version)
            .GreaterThanOrEqualTo(0).WithMessage(OnboardingMessages.VersionInvalid);

        RuleFor(x => x)
            .CustomAsync(async (command, context, cancellationToken) =>
            {
                var normalizedEmails = command.Users
                    .Select(x => x.Email.Trim().ToUpperInvariant())
                    .ToArray();

                if (normalizedEmails.Distinct(StringComparer.Ordinal).Count() != normalizedEmails.Length)
                {
                    context.AddFailure(nameof(command.Users), OnboardingMessages.DuplicateInvitationEmail);
                    return;
                }

                var emailsInUse = await onboardingRepository.GetEmailsInUseAsync(
                    normalizedEmails,
                    currentUser.Id,
                    cancellationToken);
                
                foreach (var email in emailsInUse)
                {
                    context.AddFailure(nameof(command.Users), string.Format(OnboardingMessages.InvitationEmailInUse, email.ToLowerInvariant()));
                }
            });
    }
}
