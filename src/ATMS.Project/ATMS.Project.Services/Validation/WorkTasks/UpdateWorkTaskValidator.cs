using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkTasks;

public class UpdateWorkTaskValidator : AbstractValidator<UpdateWorkTaskCommand>
{
    private readonly IWorkTaskRepository _workTaskRepository;

    public UpdateWorkTaskValidator(
        IWorkTaskRepository workTaskRepository,
        IDictionariesRepository dictionariesRepository)
    {
        _workTaskRepository = workTaskRepository;

        RuleFor(command => command.ProjectId)
            .NotEmpty().WithMessage(WorkTaskMessages.ProjectRequired);

        RuleFor(command => command.WorkTaskId)
            .NotEmpty().WithMessage(WorkTaskMessages.TaskRequired);

        RuleFor(command => command.StatusId).Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage(WorkTaskMessages.StatusRequired)
            .MustAsync(dictionariesRepository.IsWorkTaskStatusExistAsync)
            .WithMessage(WorkTaskMessages.StatusUnsupported);

        RuleFor(command => command.WorkTicketId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTaskMessages.TicketRequired)
            .MustAsync(IsTicketExistsAsync).WithMessage(WorkTaskMessages.TicketNotFound)
            .When(
                command => command.ProjectId != Guid.Empty && !command.ParentWorkTaskId.HasValue);

        RuleFor(command => command.ParentWorkTaskId)
            .CustomAsync(ValidateParentAsync)
            .When(command => command.ProjectId != Guid.Empty && command.WorkTaskId != Guid.Empty);

        RuleFor(command => command)
            .SetValidator(new WorkTaskValidator(workTaskRepository, dictionariesRepository));
    }

    private Task<bool> IsTicketExistsAsync(
        UpdateWorkTaskCommand command,
        Guid ticketId,
        CancellationToken cancellationToken)
    {
        return _workTaskRepository.IsWorkTicketExistAsync(command.ProjectId, ticketId, cancellationToken);
    }

    private async Task ValidateParentAsync(
        Guid? parentWorkTaskId,
        ValidationContext<UpdateWorkTaskCommand> context,
        CancellationToken cancellationToken)
    {
        var command = context.InstanceToValidate;
        if (!parentWorkTaskId.HasValue)
        {
            return;
        }

        var field = nameof(UpdateWorkTaskCommand.ParentWorkTaskId);

        if (parentWorkTaskId.Value == command.WorkTaskId)
        {
            context.AddFailure(field, WorkTaskMessages.ParentIsSelf);
            return;
        }

        var parent = await _workTaskRepository.FindParentAsync(
            command.ProjectId,
            parentWorkTaskId.Value,
            cancellationToken);

        if (parent is null)
        {
            context.AddFailure(field, WorkTaskMessages.ParentNotFound);
            return;
        }

        if (parent.ParentWorkTaskId.HasValue)
        {
            context.AddFailure(field, WorkTaskMessages.ParentIsSubtask);
            return;
        }

        if (await _workTaskRepository.HasChildrenAsync(
                command.ProjectId,
                command.WorkTaskId,
                cancellationToken))
        {
            context.AddFailure(field, WorkTaskMessages.ParentHasSubtasks);
        }
    }
}
