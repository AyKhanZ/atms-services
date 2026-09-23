using ATMS.Data.Criteria.Interfaces;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkTasks;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IWorkTaskBoardRepository
{
    Task<WorkTasksQueryResult> GetManyAsync(
        ICriteria<WorkTask> criteria,
        IKeysetPagination<WorkTask> pagination,
        CancellationToken cancellationToken);

    Task<Dictionary<int, int>> GetCountsByStatusAsync(ICriteria<WorkTask> criteria, CancellationToken cancellationToken);

    Task<WorkTaskBoardAssignee[]> GetAssigneesAsync(
        Guid userId,
        bool isSuperAdmin,
        IReadOnlyCollection<Guid> projectIds,
        CancellationToken cancellationToken);
}
