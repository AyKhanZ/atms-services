using ATMS.Application.Dispatcher.Validation;
using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Conflict;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Onboarding;

public class SaveSecurityValidator : AbstractValidator<SaveSecurityCommand>
{
    private readonly ICurrentUser _currentUser;
    private readonly IOnboardingRepository _onboardingRepository;

    public SaveSecurityValidator(ICurrentUser currentUser, IOnboardingRepository onboardingRepository)
    {
        _currentUser = currentUser;
        _onboardingRepository = onboardingRepository;

        RuleFor(x => x.Password).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.PasswordRequired)
            .MinimumLength(10).WithMessage(AccountMessages.PasswordTooShort)
            .MaximumLength(40).WithMessage(AccountMessages.PasswordTooLong)
            .Must(password => PasswordHelper.IsValid(password, 10, true))
            .WithMessage(AccountMessages.PasswordInvalidFormat);
        
        RuleFor(x => x.ConfirmPassword).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.ConfirmPasswordRequired)
            .Equal(x => x.Password).WithMessage(AccountMessages.PasswordsNotMatches);
        RuleFor(x => x).CustomAsync(ValidateOnboardingAsync);
    }

    private async Task ValidateOnboardingAsync(
        SaveSecurityCommand command,
        ValidationContext<SaveSecurityCommand> context,
        CancellationToken cancellationToken)
    {
        var progress = await _onboardingRepository.GetAsync(_currentUser.Id, cancellationToken)
            ?? throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);

        if (progress.User.HasCompletedOnboarding)
        {
            throw new ConflictException(OnboardingMessages.OnboardingAlreadyCompleted);
        }

        if (progress.Version != command.Version)
        {
            throw new ConflictException(OnboardingMessages.OnboardingConcurrencyConflict);
        }
    }
}
