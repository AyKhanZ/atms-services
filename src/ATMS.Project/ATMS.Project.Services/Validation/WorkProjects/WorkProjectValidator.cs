using ATMS.Application.Exceptions.Resources;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Data.Criteria.Users;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;

namespace ATMS.Project.Services.Validation.WorkProjects;

public class WorkProjectValidator : AbstractValidator<WorkProjectCommand>
{
    private const int MaxParticipants = 20;

    private readonly IWorkProjectRepository _workProjectRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IDictionariesRepository _dictionariesRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public WorkProjectValidator(
        IWorkProjectRepository workProjectRepository,
        IOrganizationRepository organizationRepository,
        IDictionariesRepository dictionariesRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository)
    {
        _workProjectRepository = workProjectRepository;
        _organizationRepository = organizationRepository;
        _dictionariesRepository = dictionariesRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;

        RuleFor(x => x.Title).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(ValidationMessages.TitleRequired)
            .MaximumLength(100).WithMessage(_ => string.Format(ValidationMessages.TitleShouldBeLessThan, 100))
            .MustAsync(IsTitleUniqueAsync).WithMessage(WorkProjectMessages.TitleAlreadyExists);

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage(_ => string.Format(WorkProjectMessages.DescriptionTooLong, 2000));

        RuleFor(x => x.OrganizationId)
            .Null().When(x => x.ProjectKindId == (int)ProjectKindEnum.Internal)
            .WithMessage(WorkProjectMessages.InternalProjectOrganizationNotAllowed);

        RuleFor(x => x.OrganizationId)
            .NotNull().When(RequiresOrganization)
            .WithMessage(WorkProjectMessages.OrganizationRequired);

        RuleFor(x => x.OrganizationId).Cascade(CascadeMode.Stop)
            .Must(x => !x.HasValue || x.Value != Guid.Empty)
            .WithMessage(WorkProjectMessages.OrganizationRequired)
            .MustAsync(IsOrganizationExistAsync).WithMessage(WorkProjectMessages.OrganizationNotFound);

        RuleFor(x => x.ProjectTypeId).Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage(WorkProjectMessages.ProjectTypeRequired)
            .MustAsync(_dictionariesRepository.IsProjectTypeExistAsync)
            .WithMessage(WorkProjectMessages.ProjectTypeUnsupported);

        RuleFor(x => x.ProjectKindId).Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage(WorkProjectMessages.ProjectKindRequired)
            .MustAsync(_dictionariesRepository.IsProjectKindExistAsync)
            .WithMessage(WorkProjectMessages.ProjectKindUnsupported);

        RuleFor(x => x.ProjectStatusId).Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage(WorkProjectMessages.ProjectStatusRequired)
            .MustAsync(_dictionariesRepository.IsProjectStatusExistAsync)
            .WithMessage(WorkProjectMessages.ProjectStatusUnsupported);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage(WorkProjectMessages.StartDateAfterEndDate);

        RuleFor(x => x.Participants)
            .Must(x => x.Length <= MaxParticipants)
            .WithMessage(string.Format(WorkProjectMessages.ParticipantsLimitExceeded, MaxParticipants))
            .Must(HaveUniqueParticipants).WithMessage(WorkProjectMessages.DuplicateParticipant);

        RuleFor(x => x.Participants)
            .NotEmpty().When(RequiresOrganization)
            .WithMessage(WorkProjectMessages.ParticipantsRequired);

        RuleForEach(x => x.Participants).ChildRules(participant =>
        {
            participant.RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(WorkProjectMessages.ParticipantRequired);
            
            participant.RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage(WorkProjectMessages.ParticipantRoleRequired);
        });

        RuleFor(x => x)
            .CustomAsync(ValidateParticipantsAsync)
            .When(ShouldValidateParticipants);
    }

    private async Task<bool> IsTitleUniqueAsync(WorkProjectCommand command, string title, CancellationToken cancellationToken)
    {
        var updateId = (command as UpdateWorkProjectCommand)?.Id;
        var normalizedTitle = title.Trim().ToLower();

        return !await _workProjectRepository.IsExistAsync(
            x => x.OrganizationId == command.OrganizationId &&
                 x.Title.ToLower() == normalizedTitle &&
                 (!updateId.HasValue || x.Id != updateId),
            cancellationToken);
    }

    private async Task<bool> IsOrganizationExistAsync(Guid? id, CancellationToken cancellationToken)
    {
        return !id.HasValue || await _organizationRepository.IsExistAsync(x => x.Id == id.Value, cancellationToken);
    }

    private bool HaveUniqueParticipants(WorkProjectParticipantCommand[] participants)
    {
        return participants.Select(x => x.UserId).Distinct().Count() == participants.Length;
    }

    private bool ShouldValidateParticipants(WorkProjectCommand command)
    {
        return command.Participants.Length is > 0 and <= MaxParticipants &&
               HaveUniqueParticipants(command.Participants) &&
               command.Participants.All(x => x.UserId != Guid.Empty && x.RoleId != Guid.Empty);
    }

    private async Task ValidateParticipantsAsync(WorkProjectCommand command, ValidationContext<WorkProjectCommand> context, CancellationToken cancellationToken)
    {
        if (command.Participants.Length == 0)
        {
            return;
        }

        var participantUserIds = command.Participants.Select(x => x.UserId).Distinct().ToArray();
        var users = await _userRepository.GetManyAsync(
            participantUserIds,
            new NotAdminCriteria<User>(),
            cancellationToken);

        if (users.Count != participantUserIds.Length)
        {
            context.AddFailure(nameof(command.Participants), WorkProjectMessages.ParticipantNotFound);
            return;
        }

        var roles = await _roleRepository.GetManyAsync(command.Participants.Select(x => x.RoleId).Distinct(), cancellationToken);

        if (roles.Count != command.Participants.Select(x => x.RoleId).Distinct().Count())
        {
            context.AddFailure(nameof(command.Participants), WorkProjectMessages.ParticipantRoleNotFound);
            return;
        }

        if (users.Any(x => x.UserType == (int)UserTypeEnum.Client && x.OrganizationId != command.OrganizationId))
        {
            context.AddFailure(nameof(command.Participants), WorkProjectMessages.ParticipantOrganizationMismatch);
        }

        var usersById = users.ToDictionary(x => x.Id);
        var internalRoleIds = new HashSet<Guid>
        {
            RoleIds.ProjectManager,
            RoleIds.BusinessConsultant,
            RoleIds.Developer
        };
        var clientRoleIds = new HashSet<Guid>
        {
            RoleIds.OrgClientManager,
            RoleIds.OrgClientViewer
        };

        var hasRoleMismatch = command.Participants.Any(participant =>
        {
            var user = usersById[participant.UserId];
            var allowedRoleIds = user.UserType == (int)UserTypeEnum.Client
                ? clientRoleIds
                : internalRoleIds;
            return !allowedRoleIds.Contains(participant.RoleId);
        });

        if (hasRoleMismatch)
        {
            context.AddFailure(nameof(command.Participants), WorkProjectMessages.ParticipantRoleMismatch);
        }
    }

    private static bool RequiresOrganization(WorkProjectCommand command)
    {
        return command.ProjectKindId > 0 && command.ProjectKindId != (int)ProjectKindEnum.Internal;
    }
}
