using ATMS.Admin.Contracts.Commands.UserProgresses;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Resources;
using FluentValidation;
using PhoneNumbers;

namespace ATMS.Admin.Service.Validation.UserProgresses;

public class PersonalInfoValidator : AbstractValidator<PersonalInfoCommand>
{
    private static readonly PhoneNumberUtil PhoneUtil = PhoneNumberUtil.GetInstance();
    private readonly IDictionariesRepository _dictionariesRepository;
    
    public PersonalInfoValidator(IDictionariesRepository dictionariesRepository)
    {
        _dictionariesRepository = dictionariesRepository;
        
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.EmailRequired)
            .EmailAddress().WithMessage(ValidationMessages.InvalidEmailFormat);
        
        RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.NameRequired)
            .MaximumLength(50).WithMessage(_ => string.Format(AccountMessages.NameShouldBeLessThan, 50));

        RuleFor(x => x.Surname).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AccountMessages.SurnameRequired)
            .MaximumLength(100).WithMessage(_ => string.Format(AccountMessages.SurnameShouldBeLessThan, 100));
        
        
        RuleFor(s => s.PhoneNumber).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ProfileMessages.PhoneNumberRequired)
            .Must(IsValidPhoneNumber)
            .WithMessage(ProfileMessages.PhoneNumberValidValue);
        
        RuleFor(s => s.Position)
            .NotEmpty().WithMessage(ProfileMessages.PositionRequired)
            .MaximumLength(50).WithMessage(string.Format(ProfileMessages.PositionMaxLength, 50));
        
        
        RuleFor(s => s.BirthDate)
            .NotEmpty().WithMessage(ProfileMessages.BirthDateRequired)
            .Must(date => date > new DateTime(1900, 1, 1)).WithMessage(ProfileMessages.BirthDateMinValue)
            .Must(date => date <= DateTime.UtcNow).WithMessage(ProfileMessages.BirthDateMaxValue)
            .Must(date => date <= DateTime.UtcNow.AddYears(-18)).WithMessage(ProfileMessages.BirthDateValidValue);
        
        
        RuleFor(s => s.Language)
            .NotEmpty().WithMessage(ProfileMessages.LanguageRequired)
            .Length(2).WithMessage(string.Format(ProfileMessages.LanguageLength, 2));
        
        RuleFor(s => s.AvatarPath)
            .NotEmpty().WithMessage(ProfileMessages.FileNameRequired)
            .MaximumLength(50).WithMessage(string.Format(ProfileMessages.FileNameMaxLength, 50));
        
        
        RuleFor(s => s.MaritalStatusId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ProfileMessages.MaritalStatusRequired)
            .MustAsync(IsMaritalStatusExistAsync).WithMessage(ProfileMessages.MaritalStatusNotSupported);
        
        RuleFor(s => s.GenderId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ProfileMessages.GenderRequired)
            .MustAsync(IsGenderExistAsync).WithMessage(ProfileMessages.GenderNotSupported);
    }
    
    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        try
        {
            var parsed = PhoneUtil.Parse(phoneNumber, null);
            return PhoneUtil.IsValidNumber(parsed);
        }
        catch (NumberParseException)
        {
            return false;
        }
    }

    private Task<bool> IsGenderExistAsync(int genderId, CancellationToken cancellationToken)
    {
        return _dictionariesRepository.IsGenderExistAsync(m => m.Id == genderId, cancellationToken);
    }

    private Task<bool> IsMaritalStatusExistAsync(int maritalStatusId, CancellationToken cancellationToken)
    {
        return _dictionariesRepository.IsMaritalStatusExistAsync(m => m.Id == maritalStatusId, cancellationToken);
    }
}