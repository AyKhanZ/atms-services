using ATMS.Project.Data.DbContexts;
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
}
