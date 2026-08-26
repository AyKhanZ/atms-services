using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Conflict;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Infrastructure.Validation;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using ATMS.Application.Dispatcher.Validation;

namespace ATMS.Admin.Service.Validation.Onboarding;

public class SavePersonalInfoValidator : BaseImageValidator<SavePersonalInfoCommand>
{
    private readonly ICurrentUser _currentUser;
    private readonly IOnboardingRepository _onboardingRepository;
    private readonly IDictionariesRepository _dictionariesRepository;

    public SavePersonalInfoValidator(
        IConfiguration configuration,
        ICurrentUser currentUser,
        IOnboardingRepository onboardingRepository,
        IDictionariesRepository dictionariesRepository) : base(configuration)
    {
        _currentUser = currentUser;
        _onboardingRepository = onboardingRepository;
        _dictionariesRepository = dictionariesRepository;

        RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.NameRequired)
            .MaximumLength(50).WithMessage(string.Format(AccountMessages.NameShouldBeLessThan, 50));

        RuleFor(x => x.Surname).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.SurnameRequired)
            .MaximumLength(100).WithMessage(string.Format(AccountMessages.SurnameShouldBeLessThan, 100));

        RuleFor(x => x.PhoneNumber).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ProfileMessages.PhoneNumberRequired)
            .MaximumLength(20).WithMessage(OnboardingMessages.PhoneNumberMaxLength)
            .Must(PhoneNumberHelper.IsValid).WithMessage(OnboardingMessages.InvalidPhoneNumber);
        
        RuleFor(x => x.Position).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ProfileMessages.PositionRequired)
            .MaximumLength(100).WithMessage(string.Format(ProfileMessages.PositionMaxLength, 100));
        
        RuleFor(x => x.BirthDate)
            .IsInDateRange(new DateOnly(1900, 1, 1), DateOnly.FromDateTime(DateTime.UtcNow))
            .Must(x => x <= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18))).WithMessage(OnboardingMessages.MinimumAge);
        
        
        RuleFor(x => x.GenderId)
            .MustAsync(IsGenderExistAsync)
            .WithMessage(OnboardingMessages.UnsupportedGender);
        
        RuleFor(x => x.MaritalStatusId)
            .MustAsync(IsMaritalStatusExistAsync)
            .WithMessage(OnboardingMessages.UnsupportedMaritalStatus);
        
        RuleFor(x => x.LanguageId)
            .MustAsync(IsLanguageExistAsync)
            .WithMessage(OnboardingMessages.UnsupportedLanguage);
        
        RuleForOptionalImage(x => x.Avatar);
        
        RuleFor(x => x).CustomAsync(ValidateOnboardingAsync);
    }

    private Task<bool> IsGenderExistAsync(int genderId, CancellationToken cancellationToken)
    {
        return _dictionariesRepository.IsGenderExistAsync(x => x.Id == genderId, cancellationToken);
    }

    private Task<bool> IsMaritalStatusExistAsync(int maritalStatusId, CancellationToken cancellationToken)
    {
        return _dictionariesRepository.IsMaritalStatusExistAsync(x => x.Id == maritalStatusId, cancellationToken);
    }

    private Task<bool> IsLanguageExistAsync(int languageId, CancellationToken cancellationToken)
    {
        return _dictionariesRepository.IsLanguageExistAsync(x => x.Id == languageId, cancellationToken);
    }

    private async Task ValidateOnboardingAsync(
        SavePersonalInfoCommand command,
        ValidationContext<SavePersonalInfoCommand> context,
        CancellationToken cancellationToken)
    {
        var progress = await _onboardingRepository.GetAsNoTrackingAsync(_currentUser.Id, cancellationToken)
            ?? throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);

        if (progress.User.HasCompletedOnboarding)
        {
            throw new ConflictException(OnboardingMessages.OnboardingAlreadyCompleted);
        }

        if (progress.Version != command.Version)
        {
            throw new ConflictException(OnboardingMessages.OnboardingConcurrencyConflict);
        }

        if (command.Avatar is null && string.IsNullOrWhiteSpace(progress.PersonalInfo?.AvatarPath))
        {
            context.AddFailure(nameof(command.Avatar), OnboardingMessages.ProfilePhotoRequired);
        }
    }
}
