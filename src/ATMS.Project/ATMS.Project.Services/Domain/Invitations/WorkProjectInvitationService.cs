using ATMS.Application.Interfaces;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Infrastructure;
using ATMS.Project.Services.Domain.Invitations.Interfaces;
using ATMS.Project.Services.Domain.Security.Interfaces;

namespace ATMS.Project.Services.Domain.Invitations;

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

            // an existing user from another organization or an employee: the invite was for a client, so refuse it
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

            // one save per invitation: history must name who invited, and it can be different people
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
