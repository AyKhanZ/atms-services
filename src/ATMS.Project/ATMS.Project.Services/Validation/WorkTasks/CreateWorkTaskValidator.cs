using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkTasks;

public class CreateWorkTaskValidator : AbstractValidator<CreateWorkTaskCommand>
{
    private readonly IWorkTaskRepository _workTaskRepository;
    
    public CreateWorkTaskValidator(
        IWorkTaskRepository workTaskRepository,
        IDictionariesRepository dictionariesRepository)
    {
        _workTaskRepository = workTaskRepository;
        
        RuleFor(command => command.ProjectId)
            .NotEmpty().WithMessage(WorkTaskMessages.ProjectRequired);

        RuleFor(command => command.WorkTicketId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTaskMessages.TicketRequired)
            .MustAsync(IsTicketExistsAsync).WithMessage(WorkTaskMessages.TicketNotFound)
            .When(command => command.ProjectId != Guid.Empty && !command.ParentWorkTaskId.HasValue);

        RuleFor(command => command.ParentWorkTaskId)
            .CustomAsync(ValidateParentAsync)
            .When(command => command.ProjectId != Guid.Empty);

        RuleFor(command => command)
            .SetValidator(new WorkTaskValidator(workTaskRepository, dictionariesRepository));
    }

    private Task<bool> IsTicketExistsAsync(CreateWorkTaskCommand command, Guid ticketId, CancellationToken cancellationToken)
    {
        return _workTaskRepository.IsWorkTicketExistAsync(command.ProjectId, ticketId, cancellationToken);
    }

    private async Task ValidateParentAsync(
        Guid? parentWorkTaskId,
        ValidationContext<CreateWorkTaskCommand> context,
        CancellationToken cancellationToken)
    {
        if (!parentWorkTaskId.HasValue)
        {
            return;
        }

        var parent = await _workTaskRepository.FindParentAsync(
            context.InstanceToValidate.ProjectId,
            parentWorkTaskId.Value,
            cancellationToken);

        if (parent is null)
        {
            context.AddFailure(nameof(CreateWorkTaskCommand.ParentWorkTaskId), WorkTaskMessages.ParentNotFound);
            return;
        }

        if (parent.ParentWorkTaskId.HasValue)
        {
            context.AddFailure(nameof(CreateWorkTaskCommand.ParentWorkTaskId), WorkTaskMessages.ParentIsSubtask);
        }
    }
}
