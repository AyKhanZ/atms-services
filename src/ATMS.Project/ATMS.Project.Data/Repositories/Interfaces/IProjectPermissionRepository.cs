using ATMS.Data.Enums;
using ATMS.Project.Data.Models.WorkProjects;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IProjectPermissionRepository
{
    Task<string[]> GetPermissionCodesAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<ProjectUserRow[]> GetUsersWithPermissionAsync(
        IReadOnlyCollection<Guid> projectIds,
        IReadOnlyCollection<Guid> userIds,
        ProjectPermissionEnum permission,
        CancellationToken cancellationToken);
}
