using ATMS.Data.Enums;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Models.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public sealed class ProjectPermissionRepository(ProjectDbContext context) : IProjectPermissionRepository
{
    public Task<string[]> GetPermissionCodesAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return context.WorkProjectParticipants
            .AsNoTracking()
            .Where(participant =>
                participant.WorkProjectId == projectId &&
                participant.UserId == userId)
            .SelectMany(participant => participant.WorkProjectParticipantRoles)
            .SelectMany(participantRole => participantRole.Role.RolePermissions)
            .Select(rolePermission => rolePermission.Permission.Code)
            .Distinct()
            .ToArrayAsync(cancellationToken);
    }

    // one query for all projects and users, the caller drops pairs that don't match
    public Task<ProjectUserRow[]> GetUsersWithPermissionAsync(
        IReadOnlyCollection<Guid> projectIds,
        IReadOnlyCollection<Guid> userIds,
        ProjectPermissionEnum permission,
        CancellationToken cancellationToken)
    {
        return context.WorkProjectParticipants
            .AsNoTracking()
            .Where(participant =>
                projectIds.Contains(participant.WorkProjectId) &&
                userIds.Contains(participant.UserId) &&
                participant.WorkProjectParticipantRoles.Any(participantRole =>
                    participantRole.Role.RolePermissions.Any(rolePermission =>
                        rolePermission.PermissionId == (int)permission)))
            .Select(participant => new ProjectUserRow(participant.WorkProjectId, participant.UserId))
            .Distinct()
            .ToArrayAsync(cancellationToken);
    }
}
