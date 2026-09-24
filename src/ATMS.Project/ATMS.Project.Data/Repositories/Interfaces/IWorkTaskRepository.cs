using ATMS.Data.Criteria.Interfaces;
using ATMS.Data.Enums;
using ATMS.Data.Criteria;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkTasks;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IWorkTaskRepository
{
    Task<WorkTasksQueryResult> GetManyAsync(
        WorkTasksByProjectCriteria criteria,
        KeysetPaginationCriteria<WorkTask> pagination,
        CancellationToken cancellationToken);

    Task<WorkTask?> GetAsync(Guid projectId, Guid workTaskId, CancellationToken cancellationToken);

    Task<WorkTask?> FindAsync(Guid projectId, Guid workTaskId, CancellationToken cancellationToken);

    Task<WorkTask?> FindParentAsync(Guid projectId, Guid parentWorkTaskId, CancellationToken cancellationToken);

    Task<bool> HasChildrenAsync(Guid projectId, Guid parentWorkTaskId, CancellationToken cancellationToken);

    Task<WorkTask[]> FindChildrenAsync(
        Guid projectId,
        Guid parentWorkTaskId,
        CancellationToken cancellationToken);

    Task<Guid[]> GetIdsByTicketsAsync(IReadOnlyCollection<Guid> workTicketIds, CancellationToken cancellationToken);

    Task<Guid[]> GetChildIdsAsync(Guid parentWorkTaskId, CancellationToken cancellationToken);

    Task<bool> IsWorkTaskExistAsync(Guid projectId, Guid workTaskId, CancellationToken cancellationToken);

    Task<bool> IsWorkTaskExistAsync(Guid workTaskId, CancellationToken cancellationToken);

    Task<bool> IsWorkTicketExistAsync(Guid projectId, Guid workTicketId, CancellationToken cancellationToken);

    Task<bool> IsProjectParticipantExistAsync(Guid projectId, Guid participantId, CancellationToken cancellationToken);

    Task<bool> IsStaffProjectParticipantExistAsync(Guid projectId, Guid participantId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, WorkTaskProgress>> GetProgressByParentAsync(
        IReadOnlyCollection<Guid> parentWorkTaskIds, CancellationToken cancellationToken);

    Task<WorkTaskProgress> GetProgressAsync(Guid parentWorkTaskId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, WorkTaskProgress>> GetProgressByTicketAsync(
        IReadOnlyCollection<Guid> workTicketIds,
        CancellationToken cancellationToken);
    Task<string?> GetTopRankAsync(int statusId, CancellationToken cancellationToken);

    Task<string?> GetNextRankAsync(int statusId, string rank, CancellationToken cancellationToken);

    Task RenumberColumnAsync(int statusId, CancellationToken cancellationToken);

    Task<Dictionary<Guid, string>> GetRanksAsync(IReadOnlyCollection<Guid> workTaskIds, ICriteria<WorkTask> criteria, CancellationToken cancellationToken);
    Task<WorkTask[]> FindByTicketAsync(Guid projectId, Guid workTicketId, CancellationToken cancellationToken);

    Task AddAsync(WorkTask workTask, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken);
}
