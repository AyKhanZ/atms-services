using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Dispatcher.Validation;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Profile;

public class UpdateSettingsValidator : AbstractValidator<UpdateSettingsCommand>
{
    private readonly IDictionariesRepository _dictionariesRepository;
    
    public UpdateSettingsValidator(IDictionariesRepository dictionariesRepository)
    {
        _dictionariesRepository = dictionariesRepository;
        
        RuleFor(s => s.Id)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);

        RuleFor(s => s.Name)
            .NotEmpty().WithMessage(ValidationMessages.NameRequired)
            .MaximumLength(50).WithMessage(string.Format(ValidationMessages.NameShouldBeLessThan, 50));

        RuleFor(s => s.Surname)
            .NotEmpty().WithMessage(AccountMessages.SurnameRequired)
            .MaximumLength(100)
            .WithMessage(_ => string.Format(AccountMessages.SurnameShouldBeLessThan, 100));

        RuleFor(s => s.PhoneNumber).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ProfileMessages.PhoneNumberRequired)
            .Must(PhoneNumberHelper.IsValid)
            .WithMessage(ProfileMessages.PhoneNumberValidValue);
        
        RuleFor(s => s.Position)
            .NotEmpty().WithMessage(ProfileMessages.PositionRequired)
            .MaximumLength(50).WithMessage(string.Format(ProfileMessages.PositionMaxLength, 50));
        
        RuleFor(s => s.BirthDate)
            .NotEmpty().WithMessage(ProfileMessages.BirthDateRequired)
            .Must(date => date > new DateTime(1900, 1, 1)).WithMessage(ProfileMessages.BirthDateMinValue)
            .Must(date => date <= DateTime.UtcNow).WithMessage(ProfileMessages.BirthDateMaxValue)
            .Must(date => date <= DateTime.UtcNow.AddYears(-18)).WithMessage(ProfileMessages.BirthDateValidValue);
        
        RuleFor(s => s.MaritalStatusId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ProfileMessages.MaritalStatusRequired)
            .MustAsync(IsMaritalStatusExistAsync).WithMessage(ProfileMessages.MaritalStatusNotSupported);
        
        RuleFor(s => s.GenderId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ProfileMessages.GenderRequired)
            .MustAsync(IsGenderExistAsync).WithMessage(ProfileMessages.GenderNotSupported);
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
