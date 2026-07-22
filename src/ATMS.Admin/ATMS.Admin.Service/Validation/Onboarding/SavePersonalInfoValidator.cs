using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Interfaces;
using ATMS.Infrastructure.Validation;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using PhoneNumbers;

namespace ATMS.Admin.Service.Validation.Onboarding;

public sealed class SavePersonalInfoValidator : BaseImageValidator<SavePersonalInfoCommand>
{
    private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();

    public SavePersonalInfoValidator(
        IConfiguration configuration,
        ICurrentUser currentUser,
        IOnboardingRepository onboardingRepository,
        IDictionariesRepository dictionariesRepository) : base(configuration)
    {
        RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.NameRequired)
            .MaximumLength(50).WithMessage(string.Format(AccountMessages.NameShouldBeLessThan, 50));

        RuleFor(x => x.Surname).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.SurnameRequired)
            .MaximumLength(100).WithMessage(string.Format(AccountMessages.SurnameShouldBeLessThan, 100));

        RuleFor(x => x.PhoneNumber).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ProfileMessages.PhoneNumberRequired)
            .MaximumLength(20).WithMessage(OnboardingMessages.PhoneNumberMaxLength)
            .Must(IsValidPhoneNumber).WithMessage(OnboardingMessages.InvalidPhoneNumber);
        
        RuleFor(x => x.Position).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ProfileMessages.PositionRequired)
            .MaximumLength(100).WithMessage(string.Format(ProfileMessages.PositionMaxLength, 100));
        
        RuleFor(x => x.BirthDate)
            .Must(x => x >= new DateOnly(1900, 1, 1)).WithMessage(OnboardingMessages.InvalidBirthDate)
            .Must(x => x <= DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18))).WithMessage(OnboardingMessages.MinimumAge);
        
        RuleFor(x => x.GenderId)
            .MustAsync((id, ct) => dictionariesRepository.IsGenderExistAsync(x => x.Id == id, ct))
            .WithMessage(OnboardingMessages.UnsupportedGender);
        
        RuleFor(x => x.MaritalStatusId)
            .MustAsync((id, ct) => dictionariesRepository.IsMaritalStatusExistAsync(x => x.Id == id, ct))
            .WithMessage(OnboardingMessages.UnsupportedMaritalStatus);
        
        RuleFor(x => x.LanguageId)
            .MustAsync((id, ct) => dictionariesRepository.IsLanguageExistAsync(x => x.Id == id, ct))
            .WithMessage(OnboardingMessages.UnsupportedLanguage);
        
        RuleFor(x => x.Version)
            .GreaterThanOrEqualTo(0).WithMessage(OnboardingMessages.VersionInvalid);
        
        RuleForOptionalImage(x => x.Avatar);
        
        RuleFor(x => x).MustAsync(async (command, ct) =>
        {
            if (command.Avatar is not null)
            {
                return true;
            }

            var progress = await onboardingRepository.GetAsync(currentUser.Id, ct);
            return !string.IsNullOrWhiteSpace(progress?.PersonalInfo?.AvatarPath);
        }).WithMessage(OnboardingMessages.ProfilePhotoRequired).WithName(nameof(SavePersonalInfoCommand.Avatar));
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        try
        {
            return PhoneUtil.IsValidNumber(PhoneUtil.Parse(phoneNumber, null));
        }
        catch (NumberParseException)
        {
            return false;
        }
    }
}
