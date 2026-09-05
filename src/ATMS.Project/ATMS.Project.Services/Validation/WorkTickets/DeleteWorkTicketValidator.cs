using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkTickets;

public class DeleteWorkTicketValidator : AbstractValidator<DeleteWorkTicketCommand>
{
    private readonly IWorkTicketRepository _workTicketRepository;

    public DeleteWorkTicketValidator(IWorkTicketRepository workTicketRepository)
    {
        _workTicketRepository = workTicketRepository;

        RuleFor(command => command.ProjectId)
            .NotEmpty().WithMessage(WorkTicketMessages.ProjectRequired);

        RuleFor(command => command.WorkTicketId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTicketMessages.TicketRequired)
            .MustAsync(HasNoTasksAsync).WithMessage(WorkTicketMessages.HasTasks)
            .When(command => command.ProjectId != Guid.Empty, ApplyConditionTo.CurrentValidator);
    }

    private async Task<bool> HasNoTasksAsync(DeleteWorkTicketCommand command, Guid workTicketId, CancellationToken cancellationToken)
    {
        return !await _workTicketRepository.HasTasksAsync(command.ProjectId, workTicketId, cancellationToken);
    }
}
