using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkTasks;

public class MoveWorkTaskValidator : AbstractValidator<MoveWorkTaskCommand>
{
    private readonly IWorkProjectRepository _workProjectRepository;
    private readonly IWorkTaskRepository _workTaskRepository;

    public MoveWorkTaskValidator(
        IWorkProjectRepository workProjectRepository,
        IWorkTaskRepository workTaskRepository,
        IDictionariesRepository dictionariesRepository)
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

        RuleFor(command => command.StatusId).Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage(WorkTaskMessages.StatusRequired)
            .MustAsync(dictionariesRepository.IsWorkTaskStatusExistAsync)
            .WithMessage(WorkTaskMessages.StatusUnsupported);

        RuleFor(command => command.PreviousWorkTaskId)
            .MustAsync(IsNeighbourExistsAsync).WithMessage(WorkTaskMessages.BoardPositionChanged)
            .When(command => command.ProjectId != Guid.Empty && command.PreviousWorkTaskId.HasValue);

        RuleFor(command => command.NextWorkTaskId)
            .MustAsync(IsNeighbourExistsAsync).WithMessage(WorkTaskMessages.BoardPositionChanged)
            .When(command => command.ProjectId != Guid.Empty && command.NextWorkTaskId.HasValue);
    }

    private Task<bool> IsProjectExistsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return _workProjectRepository.IsExistAsync(project => project.Id == projectId, cancellationToken);
    }

    private Task<bool> IsWorkTaskExistsAsync(MoveWorkTaskCommand command, Guid workTaskId, CancellationToken cancellationToken)
    {
        return _workTaskRepository.IsWorkTaskExistAsync(command.ProjectId, workTaskId, cancellationToken);
    }

    private Task<bool> IsNeighbourExistsAsync(MoveWorkTaskCommand command, Guid? workTaskId, CancellationToken cancellationToken)
    {
        return _workTaskRepository.IsWorkTaskExistAsync(command.ProjectId, workTaskId!.Value, cancellationToken);
    }
}
