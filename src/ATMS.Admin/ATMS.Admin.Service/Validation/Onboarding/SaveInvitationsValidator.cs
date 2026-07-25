using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Conflict;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Data.Constants;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Onboarding;

public class SaveInvitationsValidator : AbstractValidator<SaveInvitationsCommand>
{
    private const int MaxInvitations = 6;
    private readonly ICurrentUser _currentUser;
    private readonly IOnboardingRepository _onboardingRepository;

    public SaveInvitationsValidator(
        ICurrentUser currentUser,
        IOnboardingRepository onboardingRepository)
    {
        _currentUser = currentUser;
        _onboardingRepository = onboardingRepository;

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

        RuleFor(x => x)
            .CustomAsync(ValidateOnboardingAsync);
    }

    private async Task ValidateOnboardingAsync(
        SaveInvitationsCommand command,
        ValidationContext<SaveInvitationsCommand> context,
        CancellationToken cancellationToken)
    {
        var progress = await _onboardingRepository.GetAsync(_currentUser.Id, cancellationToken)
            ?? throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);

        if (progress.User.HasCompletedOnboarding)
        {
            throw new ConflictException(OnboardingMessages.OnboardingAlreadyCompleted);
        }

        if (_currentUser.RoleId != RoleIds.ClientManager)
        {
            throw new ConflictException(OnboardingMessages.InvitationsManagerOnly);
        }

        if (progress.Version != command.Version)
        {
            throw new ConflictException(OnboardingMessages.OnboardingConcurrencyConflict);
        }

        var normalizedEmails = command.Users
            .Select(x => x.Email.Trim().ToUpperInvariant())
            .ToArray();

        if (normalizedEmails.Distinct(StringComparer.Ordinal).Count() != normalizedEmails.Length)
        {
            context.AddFailure(nameof(command.Users), OnboardingMessages.DuplicateInvitationEmail);
            return;
        }

        var emailsInUse = await _onboardingRepository.GetEmailsInUseAsync(
            normalizedEmails,
            _currentUser.Id,
            cancellationToken);

        foreach (var email in emailsInUse)
        {
            context.AddFailure(nameof(command.Users), string.Format(OnboardingMessages.InvitationEmailInUse, email.ToLowerInvariant()));
        }
    }
}
