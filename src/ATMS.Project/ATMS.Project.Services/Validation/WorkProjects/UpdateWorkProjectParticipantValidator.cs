using ATMS.Application.Exceptions.Resources;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkProjects;

public class UpdateWorkProjectParticipantValidator : AbstractValidator<UpdateWorkProjectParticipantCommand>
{
    private readonly IWorkProjectRepository workProjectRepository;
    private readonly IRoleRepository roleRepository;

    public UpdateWorkProjectParticipantValidator(
        IWorkProjectRepository workProjectRepository,
        IRoleRepository roleRepository)
    {
        this.workProjectRepository = workProjectRepository;
        this.roleRepository = roleRepository;

        RuleFor(x => x.ProjectId).NotEmpty().WithMessage(ValidationMessages.IdRequired);
        RuleFor(x => x.ParticipantId).NotEmpty().WithMessage(ValidationMessages.IdRequired);
        RuleFor(x => x.RoleId).NotEmpty().WithMessage(WorkProjectMessages.ParticipantRoleRequired);
        RuleFor(x => x).CustomAsync(ValidateAsync);
    }

    private async Task ValidateAsync(
        UpdateWorkProjectParticipantCommand command,
        ValidationContext<UpdateWorkProjectParticipantCommand> context,
        CancellationToken cancellationToken)
    {
        if (command.ProjectId == Guid.Empty || command.ParticipantId == Guid.Empty || command.RoleId == Guid.Empty)
        {
            return;
        }

        var project = await workProjectRepository.FindAsync(command.ProjectId, cancellationToken);
        var participant = project?.WorkProjectParticipants.FirstOrDefault(x => x.Id == command.ParticipantId);
        if (participant is null)
        {
            return;
        }

        var role = (await roleRepository.GetManyAsync([command.RoleId], cancellationToken)).SingleOrDefault();
        if (role is null)
        {
            context.AddFailure(nameof(command.RoleId), WorkProjectMessages.ParticipantRoleNotFound);
            return;
        }

        AddWorkProjectParticipantValidator.ValidateUserAndRole(
            project!,
            participant.User,
            command.RoleId,
            context);
    }
}
