using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Validation;
using ATMS.Project.Contracts.Commands.Organizations;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ATMS.Project.Services.Validation.Organizations;

public class CreateOrganizationValidator : BaseImageValidator<CreateOrganizationCommand>
{
    public CreateOrganizationValidator(
        IOrganizationRepository organizationRepository,
        IConfiguration configuration) : base(configuration)
    {
        RuleFor(x => x).SetValidator(new OrganizationValidator(organizationRepository));
        RuleForOptionalImage(
            x => x.Logo,
            ValidationMessages.ImageEmpty,
            ValidationMessages.ImageTooLarge,
            ValidationMessages.ImageUnsupportedFormat);
    }
}