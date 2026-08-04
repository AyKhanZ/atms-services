using ATMS.Application.Exceptions.Resources;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkProjects;

public class UpdateWorkProjectValidator : AbstractValidator<UpdateWorkProjectCommand>
{
    public UpdateWorkProjectValidator(
        IWorkProjectRepository workProjectRepository,
        IOrganizationRepository organizationRepository,
        IDictionariesRepository dictionariesRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);

        RuleFor(x => x).SetValidator(new WorkProjectValidator(
            workProjectRepository,
            organizationRepository,
            dictionariesRepository,
            userRepository,
            roleRepository));
    }
}
