namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IProjectPermissionRepository
{
    Task<bool> HasClientRoleAsync(Guid projectId, Guid userId, CancellationToken cancellationToken);

    Task<string[]> GetPermissionCodesAsync(
        Guid projectId,
        Guid userId,
        CancellationToken cancellationToken);
}
