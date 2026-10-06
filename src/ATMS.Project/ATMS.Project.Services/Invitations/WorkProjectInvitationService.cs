using ATMS.Application.Interfaces;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Caching;
using ATMS.Project.Services.Invitations.Interfaces;
using ATMS.Project.Services.Security.Interfaces;

namespace ATMS.Project.Services.Invitations;

// Settles the project invitations waiting for this email when Project hears about the user: a new
// account, or an existing one Admin announced again because the invitation came a moment too late.
public sealed class WorkProjectInvitationService(
    IWorkProjectInvitationRepository invitationRepository,
    IWorkProjectRepository workProjectRepository,
    IAuditActorScope auditActor,
    ICacheService cache,
    IProjectPermissionService projectPermissionService) : IWorkProjectInvitationService
{
    public async Task SettlePendingAsync(User user, CancellationToken cancellationToken)
    {
        var invitations = await invitationRepository.GetPendingByEmailAsync(
            user.NormalizedEmail,
            cancellationToken);

        foreach (var invitation in invitations)
        {
            var project = await workProjectRepository.FindAsync(invitation.WorkProjectId, cancellationToken);
            var accepted = false;

            // An existing user may belong to another organization or be an employee: the invitation
            // promised a client of this project's organization, so it is turned down, not bent.
            if (project is not null &&
                project.OrganizationId is not null &&
                project.OrganizationId == user.OrganizationId &&
                user.UserType == (int)UserTypeEnum.Client &&
                project.WorkProjectParticipants.All(x => x.UserId != user.Id))
            {
                project.WorkProjectParticipants.Add(new WorkProjectParticipant
                {
                    UserId = user.Id,
                    WorkProjectParticipantRoles =
                    [
                        new WorkProjectParticipantRole
                        {
                            RoleId = invitation.RoleId
                        }
                    ]
                });
                workProjectRepository.Touch(project);
                accepted = true;
            }

            invitation.Status = accepted
                ? (int)WorkProjectInvitationStatusEnum.Accepted
                : (int)WorkProjectInvitationStatusEnum.Rejected;
            invitation.ProcessedAt = DateTime.UtcNow;

            // One save per invitation: history names the person who invited, and two invitations
            // to one email can come from different people.
            auditActor.ActAs(invitation.InvitedById);
            await workProjectRepository.SaveAsync(cancellationToken);

            await cache.RemoveWorkProjectAsync(invitation.WorkProjectId, cancellationToken);
            if (accepted)
            {
                await projectPermissionService.RemoveUserPermissionsAsync(
                    invitation.WorkProjectId,
                    user.Id,
                    cancellationToken);
            }
        }
    }
}
