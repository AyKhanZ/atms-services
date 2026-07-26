using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Conflict;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Onboarding;

public class CompleteOnboardingValidator : AbstractValidator<CompleteOnboardingCommand>
{
    private readonly ICurrentUser _currentUser;
    private readonly IOnboardingRepository _onboardingRepository;

    public CompleteOnboardingValidator(ICurrentUser currentUser, IOnboardingRepository onboardingRepository)
    {
        _currentUser = currentUser;
        _onboardingRepository = onboardingRepository;

        RuleFor(x => x).CustomAsync(ValidateOnboardingAsync);
    }

    private async Task ValidateOnboardingAsync(
        CompleteOnboardingCommand command,
        ValidationContext<CompleteOnboardingCommand> context,
        CancellationToken cancellationToken)
    {
        var progress = await _onboardingRepository.GetAsNoTrackingAsync(_currentUser.Id, cancellationToken)
            ?? throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);

        if (progress.User.HasCompletedOnboarding)
        {
            return;
        }

        if (progress.Version != command.Version)
        {
            throw new ConflictException(OnboardingMessages.OnboardingConcurrencyConflict);
        }

        if (progress.PersonalInfoStatus != OnboardingStepStatusEnum.Completed || progress.PersonalInfo is null)
        {
            throw new ConflictException(OnboardingMessages.PersonalInfoIncomplete);
        }

        if (progress.SecurityStatus != OnboardingStepStatusEnum.Completed || progress.PendingPasswordHash is null)
        {
            throw new ConflictException(OnboardingMessages.SecurityIncomplete);
        }

        if (_currentUser.RoleId == RoleIds.ClientManager &&
            progress.InvitationsStatus == OnboardingStepStatusEnum.NotStarted)
        {
            throw new ConflictException(OnboardingMessages.InvitationsIncomplete);
        }
    }
}
