using ATMS.Application.Exceptions.Resources;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkProjects;

public class AddWorkProjectParticipantValidator : AbstractValidator<AddWorkProjectParticipantCommand>
{
    private const int MaxParticipants = 20;

    private readonly IWorkProjectRepository workProjectRepository;
    private readonly IUserRepository userRepository;
    private readonly IRoleRepository roleRepository;

    public AddWorkProjectParticipantValidator(
        IWorkProjectRepository workProjectRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        this.workProjectRepository = workProjectRepository;
        this.userRepository = userRepository;
        this.roleRepository = roleRepository;

        RuleFor(x => x.ProjectId).NotEmpty().WithMessage(ValidationMessages.IdRequired);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(WorkProjectMessages.ParticipantRequired);
        RuleFor(x => x.RoleId).NotEmpty().WithMessage(WorkProjectMessages.ParticipantRoleRequired);
        RuleFor(x => x).CustomAsync(ValidateAsync);
    }

    private async Task ValidateAsync(
        AddWorkProjectParticipantCommand command,
        ValidationContext<AddWorkProjectParticipantCommand> context,
        CancellationToken cancellationToken)
    {
        if (command.ProjectId == Guid.Empty || command.UserId == Guid.Empty || command.RoleId == Guid.Empty)
        {
            return;
        }

        var project = await workProjectRepository.FindAsync(command.ProjectId, cancellationToken);
        if (project is null)
        {
            return;
        }

        if (project.WorkProjectParticipants.Count >= MaxParticipants)
        {
            context.AddFailure(nameof(command.UserId), string.Format(WorkProjectMessages.ParticipantsLimitExceeded, MaxParticipants));
            return;
        }

        if (project.WorkProjectParticipants.Any(x => x.UserId == command.UserId))
        {
            context.AddFailure(nameof(command.UserId), WorkProjectMessages.DuplicateParticipant);
            return;
        }

        var user = (await userRepository.GetManyAsync([command.UserId], cancellationToken)).SingleOrDefault();
        if (user is null)
        {
            context.AddFailure(nameof(command.UserId), WorkProjectMessages.ParticipantNotFound);
            return;
        }

        var role = (await roleRepository.GetManyAsync([command.RoleId], cancellationToken)).SingleOrDefault();
        if (role is null)
        {
            context.AddFailure(nameof(command.RoleId), WorkProjectMessages.ParticipantRoleNotFound);
            return;
        }

        ValidateUserAndRole(project, user, command.RoleId, context);
    }

    internal static void ValidateUserAndRole<T>(
        WorkProject project,
        User user,
        Guid roleId,
        ValidationContext<T> context)
    {
        if (user.UserType == (int)UserTypeEnum.Client && user.OrganizationId != project.OrganizationId)
        {
            context.AddFailure(WorkProjectMessages.ParticipantOrganizationMismatch);
        }

        var allowedRoleIds = user.UserType == (int)UserTypeEnum.Client
            ? new HashSet<Guid> { RoleIds.OrgClientManager, RoleIds.OrgClientViewer }
            : new HashSet<Guid> { RoleIds.ProjectManager, RoleIds.BusinessConsultant, RoleIds.Developer };

        if (!allowedRoleIds.Contains(roleId))
        {
            context.AddFailure(WorkProjectMessages.ParticipantRoleMismatch);
        }
    }
}
