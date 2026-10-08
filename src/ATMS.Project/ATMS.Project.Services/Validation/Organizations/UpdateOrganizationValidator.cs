using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Options;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Validation;
using ATMS.Project.Contracts.Commands.Organizations;
using ATMS.Project.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Organizations;

public sealed class UpdateOrganizationValidator : BaseImageValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationValidator(
        IOrganizationRepository organizationRepository,
        IOptions<ImagesOptions> imagesOptions) : base(imagesOptions)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);
        
        RuleFor(x => x).SetValidator(new OrganizationValidator(organizationRepository));
        RuleForOptionalImage(
            x => x.Logo,
            ValidationMessages.ImageEmpty,
            ValidationMessages.ImageTooLarge,
            ValidationMessages.ImageUnsupportedFormat);
    }
}