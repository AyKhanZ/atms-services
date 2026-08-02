using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Validation;
using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Data.Repositories.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace ATMS.Project.Services.Validation.Organization;

public class UpdateOrganizationValidator : BaseImageValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationValidator(
        IOrganizationRepository organizationRepository,
        IConfiguration configuration) : base(configuration)
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