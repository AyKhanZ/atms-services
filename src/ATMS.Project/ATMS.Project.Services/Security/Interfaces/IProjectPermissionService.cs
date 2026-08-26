using ATMS.Data.Enums;

namespace ATMS.Project.Services.Security.Interfaces;

public interface IProjectPermissionService
{
    Task<IReadOnlySet<string>> GetPermissionCodesAsync(
        Guid projectId,
        CancellationToken cancellationToken);

    Task<bool> HasAnyPermissionAsync(
        Guid projectId,
        IReadOnlyCollection<ProjectPermissionEnum> permissions,
        CancellationToken cancellationToken);

    Task RemoveUserPermissionsAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken);

    Task RemoveProjectPermissionsAsync(
        Guid projectId,
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken);
}
