using ATMS.Project.Contracts.Commands.Organization;
using ATMS.Project.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Project.Services.Validation.Organization;

public class UpdateOrganizationValidator
    : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationValidator(IOrganizationRepository organizationRepository)
    {
        RuleFor(x => x).SetValidator(new OrganizationValidator(organizationRepository));
    }
}
