using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkGroups;
using ATMS.Data.Criteria;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IWorkGroupRepository
{
    Task<WorkGroupsQueryResult> GetGroupsAsync(Guid projectId, CancellationToken cancellationToken);

    Task<KeysetPagedResult<WorkGroup>> GetMilestonesAsync(
        ACriteria<WorkGroup> criteria,
        KeysetPaginationCriteria<WorkGroup> pagination,
        CancellationToken cancellationToken);

    Task<WorkGroup?> FindAsync(Guid projectId, Guid workGroupId, CancellationToken cancellationToken);

    Task<bool> IsRootExistAsync(Guid projectId, Guid workGroupId, CancellationToken cancellationToken);

    Task<bool> IsSiblingTitleExistAsync(
        Guid projectId,
        Guid? parentWorkGroupId,
        string normalizedTitle,
        Guid? excludedWorkGroupId,
        CancellationToken cancellationToken);

    Task<bool> HasChildrenAsync(Guid workGroupId, CancellationToken cancellationToken);

    Task<bool> HasTicketsAsync(Guid workGroupId, CancellationToken cancellationToken);

    Task CreateAsync(WorkGroup workGroup, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
