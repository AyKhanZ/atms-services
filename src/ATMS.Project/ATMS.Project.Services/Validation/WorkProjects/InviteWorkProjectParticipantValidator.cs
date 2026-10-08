using ATMS.Application.Exceptions.Resources;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkProjects;

public sealed class InviteWorkProjectParticipantValidator : AbstractValidator<InviteWorkProjectParticipantCommand>
{
    private readonly IWorkProjectRepository _workProjectRepository;
    private readonly IWorkProjectInvitationRepository _invitationRepository;
    private readonly IUserRepository _userRepository;

    public InviteWorkProjectParticipantValidator(
        IWorkProjectRepository workProjectRepository,
        IWorkProjectInvitationRepository invitationRepository,
        IUserRepository userRepository)
    {
        _workProjectRepository = workProjectRepository;
        _invitationRepository = invitationRepository;
        _userRepository = userRepository;

        RuleFor(x => x.ProjectId).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ValidationMessages.IdRequired)
            .MustAsync(IsProjectExistsAsync).WithMessage(WorkProjectMessages.NotFound);

        RuleFor(x => x.Name).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ValidationMessages.NameRequired)
            .MaximumLength(50).WithMessage(_ => string.Format(ValidationMessages.NameShouldBeLessThan, 50));

        RuleFor(x => x.Surname).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ValidationMessages.SurnameRequired)
            .MaximumLength(100).WithMessage(_ => string.Format(ValidationMessages.SurnameShouldBeLessThan, 100));

        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ValidationMessages.EmailRequired)
            .MaximumLength(100).WithMessage(_ => string.Format(ValidationMessages.EmailShouldBeLessThan, 100))
            .EmailAddress().WithMessage(ValidationMessages.InvalidEmailFormat)
            .CustomAsync(ValidateProjectAsync)
            .When(x => x.ProjectId != Guid.Empty, ApplyConditionTo.CurrentValidator);
    }

    private Task<bool> IsProjectExistsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        return _workProjectRepository.IsExistAsync(project => project.Id == projectId, cancellationToken);
    }

    private async Task ValidateProjectAsync(
        string email,
        ValidationContext<InviteWorkProjectParticipantCommand> context,
        CancellationToken cancellationToken)
    {
        var project = await _workProjectRepository.FindAsync(context.InstanceToValidate.ProjectId, cancellationToken);
        if (project is null)
        {
            return;
        }

        if (project.OrganizationId is null)
        {
            context.AddFailure(WorkProjectMessages.InvitationInternalProject);
            return;
        }

        var invitations = await _invitationRepository.GetLivePendingAsync(project.Id, cancellationToken);
        if (project.WorkProjectParticipants.Count + invitations.Count >= WorkProjectParticipantLimit.Max)
        {
            context.AddFailure(string.Format(WorkProjectMessages.ParticipantsLimitExceeded, WorkProjectParticipantLimit.Max));
            return;
        }

        var normalizedEmail = email.Trim().ToUpperInvariant();
        if (project.WorkProjectParticipants.Any(x => x.User.NormalizedEmail == normalizedEmail))
        {
            context.AddFailure(WorkProjectMessages.InvitationAlreadyParticipant);
            return;
        }

        if (invitations.Any(x => x.NormalizedEmail == normalizedEmail))
        {
            context.AddFailure(WorkProjectMessages.InvitationAlreadySent);
            return;
        }

        if (await _userRepository.IsEmailTakenAsync(normalizedEmail, cancellationToken))
        {
            context.AddFailure(WorkProjectMessages.InvitationEmailInUse);
        }
    }
}
