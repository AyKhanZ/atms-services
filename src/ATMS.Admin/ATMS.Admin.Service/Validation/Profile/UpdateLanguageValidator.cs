using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Service.Resources;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Profile;

public class UpdateLanguageValidator : AbstractValidator<UpdateLanguageCommand>
{
    public UpdateLanguageValidator()
    {
        RuleFor(s => s.Language)
            .NotEmpty().WithMessage(ProfileMessages.LanguageRequired)
            .Length(2).WithMessage(string.Format(ProfileMessages.LanguageLength, 2));
    }
}
