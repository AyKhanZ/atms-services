using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Organization;

public class CreateOrganizationValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationValidator(IOrganizationRepository organizationRepository)
    {
        RuleFor(x => x).SetValidator(new OrganizationValidator(organizationRepository));
    }
}