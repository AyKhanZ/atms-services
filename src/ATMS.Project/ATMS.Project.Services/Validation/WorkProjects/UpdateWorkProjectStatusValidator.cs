using ATMS.Application.Exceptions.Resources;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkProjects;

public class UpdateWorkProjectStatusValidator : AbstractValidator<UpdateWorkProjectStatusCommand>
{
    public UpdateWorkProjectStatusValidator(IDictionariesRepository dictionariesRepository)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired);

        RuleFor(x => x.ProjectStatusId).Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage(WorkProjectMessages.ProjectStatusRequired)
            .MustAsync(dictionariesRepository.IsProjectStatusExistAsync)
            .WithMessage(WorkProjectMessages.ProjectStatusUnsupported);
    }
}
