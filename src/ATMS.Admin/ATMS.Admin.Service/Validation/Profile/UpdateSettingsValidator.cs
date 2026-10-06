using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Dispatcher.Validation;
using ATMS.Application.Interfaces;
using ATMS.Data.Constants;
using ATMS.Infrastructure.Validation;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace ATMS.Admin.Service.Validation.Profile;

public sealed class UpdateSettingsValidator : BaseImageValidator<UpdateSettingsCommand>
{
    private readonly IDictionariesRepository _dictionariesRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateSettingsValidator(
        IConfiguration configuration,
        IDictionariesRepository dictionariesRepository,
        IUserRepository userRepository,
        ICurrentUser currentUser) : base(configuration)
    {
        _dictionariesRepository = dictionariesRepository;
        _userRepository = userRepository;
        _currentUser = currentUser;

        RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ValidationMessages.NameRequired)
            .MaximumLength(50).WithMessage(string.Format(ValidationMessages.NameShouldBeLessThan, 50));

        RuleFor(x => x.Surname).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ValidationMessages.SurnameRequired)
            .MaximumLength(100).WithMessage(string.Format(ValidationMessages.SurnameShouldBeLessThan, 100));

        RuleFor(x => x.PhoneNumber).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ProfileMessages.PhoneNumberRequired)
            .MaximumLength(20).WithMessage(OnboardingMessages.PhoneNumberMaxLength)
            .Must(PhoneNumberHelper.IsValid).WithMessage(OnboardingMessages.InvalidPhoneNumber);

        RuleFor(x => x.Position).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ProfileMessages.PositionRequired)
            .MaximumLength(100).WithMessage(string.Format(ProfileMessages.PositionMaxLength, 100));

        RuleFor(x => x.BirthDate)
            .IsInDateRange(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-100)), DateOnly.FromDateTime(DateTime.UtcNow))
            .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)))
            .WithMessage(OnboardingMessages.MinimumAge);

        RuleFor(x => x.GenderId)
            .MustAsync((id, token) => _dictionariesRepository.IsGenderExistAsync(g => g.Id == id, token))
            .WithMessage(OnboardingMessages.UnsupportedGender);

        RuleFor(x => x.MaritalStatusId)
            .MustAsync((id, token) => _dictionariesRepository.IsMaritalStatusExistAsync(m => m.Id == id, token))
            .WithMessage(OnboardingMessages.UnsupportedMaritalStatus);

        RuleFor(x => x.LanguageId)
            .MustAsync((id, token) => _dictionariesRepository.IsLanguageExistAsync(l => l.Id == id, token))
            .WithMessage(OnboardingMessages.UnsupportedLanguage);

        RuleForOptionalImage(x => x.Avatar);

        RuleFor(x => x.Avatar).MustAsync(async (avatar, token) =>
                avatar is not null || HasOwnPhoto((await _userRepository.GetAsync(_currentUser.Id, token))?.AvatarPath))
            .WithMessage(OnboardingMessages.ProfilePhotoRequired);
    }

    // The shared placeholder is what an account has before anyone chose a photo; it does not count.
    private static bool HasOwnPhoto(string? avatarPath)
    {
        return !string.IsNullOrWhiteSpace(avatarPath) && avatarPath != DefaultValues.UserAvatar;
    }
}
