using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkTasks;

public class UpdateWorkTaskValidator : AbstractValidator<UpdateWorkTaskCommand>
{
    public UpdateWorkTaskValidator(
        IWorkTaskRepository workTaskRepository,
        IDictionariesRepository dictionariesRepository)
    {
        RuleFor(command => command.ProjectId)
            .NotEmpty().WithMessage(WorkTaskMessages.ProjectRequired);

        RuleFor(command => command.WorkTaskId)
            .NotEmpty().WithMessage(WorkTaskMessages.TaskRequired);

        RuleFor(command => command.StatusId).Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage(WorkTaskMessages.StatusRequired)
            .MustAsync(dictionariesRepository.IsWorkTaskStatusExistAsync).WithMessage(WorkTaskMessages.StatusUnsupported);

        RuleFor(command => command)
            .SetValidator(new WorkTaskValidator(workTaskRepository, dictionariesRepository));
    }
}
