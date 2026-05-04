using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Resources;
using FluentValidation;

namespace ATMS.Admin.Service.Validation.Profile;

public class UpdatePhotoValidator : AbstractValidator<UpdatePhotoCommand>
{
    public UpdatePhotoValidator()
    {
        RuleFor(s => s.Id)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);

        RuleFor(s => s.FileName)
            .NotEmpty().WithMessage(ProfileMessages.FileNameRequired)
            .MaximumLength(50).WithMessage(string.Format(ProfileMessages.FileNameMaxLength, 50));
    }
}