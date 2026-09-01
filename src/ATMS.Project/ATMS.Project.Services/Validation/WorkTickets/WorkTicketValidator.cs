using ATMS.Application.Dispatcher.Validation;
using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkTickets;

public class WorkTicketValidator : AbstractValidator<WorkTicketCommand>
{
    private readonly IWorkTicketRepository _workTicketRepository;

    public WorkTicketValidator(
        IWorkTicketRepository workTicketRepository,
        IDictionariesRepository dictionariesRepository)
    {
        _workTicketRepository = workTicketRepository;

        RuleFor(command => command.Title).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTicketMessages.TitleRequired)
            .Must(title => !string.IsNullOrWhiteSpace(title)).WithMessage(WorkTicketMessages.TitleRequired)
            .Must(title => title.Trim().Length <= 100)
            .WithMessage(_ => string.Format(WorkTicketMessages.TitleTooLong, 100));

        RuleFor(command => command.Description)
            .MaximumLength(2000)
            .WithMessage(string.Format(WorkTicketMessages.DescriptionTooLong, 2000));

        RuleFor(command => command.MilestoneId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTicketMessages.MilestoneRequired)
            .MustAsync(IsMilestoneValidAsync)
            .When(command => command.ProjectId != Guid.Empty)
            .WithMessage(WorkTicketMessages.MilestoneNotFound);

        RuleFor(command => command.WorkTicketTypeId).Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage(WorkTicketMessages.TypeRequired)
            .MustAsync(dictionariesRepository.IsWorkTicketTypeExistAsync)
            .WithMessage(WorkTicketMessages.TypeUnsupported);

        RuleFor(command => command.PriorityId).Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage(WorkTicketMessages.PriorityRequired)
            .MustAsync(dictionariesRepository.IsWorkItemPriorityExistAsync)
            .WithMessage(WorkTicketMessages.PriorityUnsupported);

        RuleFor(command => command.Deadline)
            .IsInDateRange()
            .WithMessage(WorkTicketMessages.DeadlineOutOfRange);

        RuleFor(command => command.AssigneeId).Cascade(CascadeMode.Stop)
            .Must(id => !id.HasValue || id.Value != Guid.Empty)
            .WithMessage(WorkTicketMessages.AssigneeNotFound)
            .MustAsync(IsAssigneeValidAsync)
            .When(command => command.ProjectId != Guid.Empty)
            .WithMessage(WorkTicketMessages.AssigneeNotFound);
    }

    private Task<bool> IsMilestoneValidAsync(WorkTicketCommand command, Guid milestoneId, CancellationToken cancellationToken)
    {
        return _workTicketRepository.IsMilestoneExistAsync(command.ProjectId, milestoneId, cancellationToken);
    }

    private async Task<bool> IsAssigneeValidAsync(WorkTicketCommand command, Guid? assigneeId, CancellationToken cancellationToken)
    {
        return !assigneeId.HasValue || await _workTicketRepository.IsProjectParticipantExistAsync(command.ProjectId, assigneeId.Value, cancellationToken);
    }
}