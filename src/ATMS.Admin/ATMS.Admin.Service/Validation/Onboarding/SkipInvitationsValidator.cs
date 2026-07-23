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

public class SkipInvitationsValidator : AbstractValidator<SkipInvitationsCommand>
{
    private readonly ICurrentUser _currentUser;
    private readonly IOnboardingRepository _onboardingRepository;

    public SkipInvitationsValidator(ICurrentUser currentUser, IOnboardingRepository onboardingRepository)
    {
        _currentUser = currentUser;
        _onboardingRepository = onboardingRepository;

        RuleFor(x => x).CustomAsync(ValidateOnboardingAsync);
    }

    private async Task ValidateOnboardingAsync(
        SkipInvitationsCommand command,
        ValidationContext<SkipInvitationsCommand> context,
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
    }
}
