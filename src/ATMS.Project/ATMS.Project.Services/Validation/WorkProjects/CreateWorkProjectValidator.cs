using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkProjects;

public class CreateWorkProjectValidator : AbstractValidator<CreateWorkProjectCommand>
{
    public CreateWorkProjectValidator(
        IWorkProjectRepository workProjectRepository,
        IOrganizationRepository organizationRepository,
        IDictionariesRepository dictionariesRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        RuleFor(x => x).SetValidator(new WorkProjectValidator(
            workProjectRepository,
            organizationRepository,
            dictionariesRepository,
            userRepository,
            roleRepository));
    }
}
