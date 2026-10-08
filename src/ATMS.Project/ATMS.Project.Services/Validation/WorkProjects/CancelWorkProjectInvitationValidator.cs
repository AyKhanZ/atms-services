using ATMS.Application.Exceptions.Resources;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkProjects;

public sealed class CancelWorkProjectInvitationValidator : AbstractValidator<CancelWorkProjectInvitationCommand>
{
    private readonly IWorkProjectRepository _workProjectRepository;
    private readonly IWorkProjectInvitationRepository _invitationRepository;

    public CancelWorkProjectInvitationValidator(
        IWorkProjectRepository workProjectRepository,
        IWorkProjectInvitationRepository invitationRepository)
    {
        _workProjectRepository = workProjectRepository;
        _invitationRepository = invitationRepository;

        RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired)
            .MustAsync(IsProjectExistsAsync).WithMessage(WorkProjectMessages.NotFound);

        // accepted a moment ago or cancelled in another tab
        RuleFor(x => x.InvitationId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired)
            .MustAsync(IsInvitationPendingAsync).WithMessage(WorkProjectMessages.InvitationNotPending)
            .When(x => x.ProjectId != Guid.Empty, ApplyConditionTo.CurrentValidator);
    }

    private Task<bool> IsProjectExistsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return _workProjectRepository.IsExistAsync(project => project.Id == projectId, cancellationToken);
    }

    private Task<bool> IsInvitationPendingAsync(
        CancelWorkProjectInvitationCommand command,
        Guid invitationId,
        CancellationToken cancellationToken)
    {
        return _invitationRepository.IsPendingAsync(command.ProjectId, invitationId, cancellationToken);
    }
}
