namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IProjectPermissionRepository
{
    Task<string[]> GetPermissionCodesAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken);
}
