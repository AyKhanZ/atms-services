using ATMS.Application.Dispatcher.Validation;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkTasks;

public class UpdateWorkTaskDeadlineValidator : AbstractValidator<UpdateWorkTaskDeadlineCommand>
{
    private readonly IWorkProjectRepository _workProjectRepository;
    private readonly IWorkTaskRepository _workTaskRepository;

    public UpdateWorkTaskDeadlineValidator(
        IWorkProjectRepository workProjectRepository,
        IWorkTaskRepository workTaskRepository)
    {
        _workProjectRepository = workProjectRepository;
        _workTaskRepository = workTaskRepository;

        RuleFor(command => command.ProjectId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTaskMessages.ProjectRequired)
            .MustAsync(IsProjectExistsAsync).WithMessage(WorkProjectMessages.NotFound);

        RuleFor(command => command.WorkTaskId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTaskMessages.TaskRequired)
            .MustAsync(IsWorkTaskExistsAsync).WithMessage(WorkTaskMessages.NotFound)
            .When(command => command.ProjectId != Guid.Empty, ApplyConditionTo.CurrentValidator);

        RuleFor(command => command.Deadline)
            .IsInDateRange().WithMessage(WorkTaskMessages.DeadlineOutOfRange);
    }

    private Task<bool> IsProjectExistsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return _workProjectRepository.IsExistAsync(project => project.Id == projectId, cancellationToken);
    }

    private Task<bool> IsWorkTaskExistsAsync(UpdateWorkTaskDeadlineCommand command, Guid workTaskId, CancellationToken cancellationToken)
    {
        return _workTaskRepository.IsWorkTaskExistAsync(command.ProjectId, workTaskId, cancellationToken);
    }
}
