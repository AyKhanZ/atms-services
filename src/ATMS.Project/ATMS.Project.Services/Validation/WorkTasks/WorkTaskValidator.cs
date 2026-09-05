using ATMS.Application.Dispatcher.Validation;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkTasks;

public class WorkTaskValidator : AbstractValidator<WorkTaskCommand>
{
    private readonly IWorkTaskRepository _workTaskRepository;

    public WorkTaskValidator(
        IWorkTaskRepository workTaskRepository,
        IDictionariesRepository dictionariesRepository)
    {
        _workTaskRepository = workTaskRepository;

        RuleFor(command => command.Title).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(WorkTaskMessages.TitleRequired)
            .Must(title => !string.IsNullOrWhiteSpace(title)).WithMessage(WorkTaskMessages.TitleRequired)
            .Must(title => title.Trim().Length <= 100).WithMessage(_ => string.Format(WorkTaskMessages.TitleTooLong, 100));

        RuleFor(command => command.Description)
            .MaximumLength(2000).WithMessage(string.Format(WorkTaskMessages.DescriptionTooLong, 2000));

        RuleFor(command => command.PriorityId).Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage(WorkTaskMessages.PriorityRequired)
            .MustAsync(dictionariesRepository.IsWorkItemPriorityExistAsync).WithMessage(WorkTaskMessages.PriorityUnsupported);

        RuleFor(command => command.Deadline)
            .IsInDateRange().WithMessage(WorkTaskMessages.DeadlineOutOfRange);

        RuleFor(command => command.AssigneeId).Cascade(CascadeMode.Stop)
            .Must(id => !id.HasValue || id.Value != Guid.Empty).WithMessage(WorkTaskMessages.AssigneeNotFound)
            .MustAsync(IsParticipantAsync).When(command => command.ProjectId != Guid.Empty).WithMessage(WorkTaskMessages.AssigneeNotFound)
            .MustAsync(IsStaffAsync).When(command => command.ProjectId != Guid.Empty).WithMessage(WorkTaskMessages.AssigneeMustBeStaff);
    }

    private async Task<bool> IsParticipantAsync(WorkTaskCommand command, Guid? assigneeId, CancellationToken cancellationToken)
    {
        return !assigneeId.HasValue || await _workTaskRepository.IsProjectParticipantExistAsync(command.ProjectId, assigneeId.Value, cancellationToken);
    }

    private async Task<bool> IsStaffAsync(WorkTaskCommand command, Guid? assigneeId, CancellationToken cancellationToken)
    {
        return !assigneeId.HasValue || await _workTaskRepository.IsStaffProjectParticipantExistAsync(command.ProjectId, assigneeId.Value, cancellationToken);
    }
}
