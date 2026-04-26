using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Resources;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Profile;

public class UpdateLanguageValidator : AbstractValidator<UpdateLanguageCommand>
{
    public UpdateLanguageValidator()
    {
        RuleFor(s => s.Id)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);

        RuleFor(command => command.Language)
            .NotEmpty().WithMessage(ProfileMessages.LanguageRequired)
            .Length(2).WithMessage(string.Format(ProfileMessages.LanguageLength, 2));
    }
}