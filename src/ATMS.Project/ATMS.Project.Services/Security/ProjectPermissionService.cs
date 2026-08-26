using ATMS.Application.Interfaces;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Security.Interfaces;

namespace ATMS.Project.Services.Security;

public sealed class ProjectPermissionService(
    ICurrentUser currentUser,
    IProjectPermissionRepository permissionRepository,
    ICacheService cache) : IProjectPermissionService
{
    public async Task<IReadOnlySet<string>> GetPermissionCodesAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var codes = await cache.GetOrSetAsync(
            CacheKeys.Project.UserPermissions(projectId, currentUser.Id),
            () => permissionRepository.GetPermissionCodesAsync(projectId, currentUser.Id, cancellationToken),
            CacheTtl.ProjectPermissions,
            cancellationToken) ?? [];

        return codes.ToHashSet(StringComparer.Ordinal);
    }

    public async Task<bool> HasAnyPermissionAsync(
        Guid projectId,
        IReadOnlyCollection<ProjectPermissionEnum> permissions,
        CancellationToken cancellationToken)
    {
        var permissionSet = await GetPermissionCodesAsync(projectId, cancellationToken);
        return permissions.Any(permission => permissionSet.Contains(permission.ToString()));
    }

    public async Task RemoveUserPermissionsAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(CacheKeys.Project.UserPermissions(projectId, userId), cancellationToken);
    }

    public async Task RemoveProjectPermissionsAsync(
        Guid projectId,
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken)
    {
        foreach (var userId in userIds.Distinct())
        {
            await RemoveUserPermissionsAsync(projectId, userId, cancellationToken);
        }
    }
}
