using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkTasks;

public class DeleteWorkTaskValidator : AbstractValidator<DeleteWorkTaskCommand>
{
    public DeleteWorkTaskValidator(IWorkTaskRepository workTaskRepository)
    {
        RuleFor(command => command.ProjectId)
            .NotEmpty().WithMessage(WorkTaskMessages.ProjectRequired);

        RuleFor(command => command.WorkTaskId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTaskMessages.TaskRequired)
            .MustAsync(async (command, workTaskId, cancellationToken) => !await workTaskRepository.HasChildrenAsync(command.ProjectId, workTaskId, cancellationToken))
            .When(command => command.ProjectId != Guid.Empty)
            .WithMessage(WorkTaskMessages.HasSubtasks);
    }
}
