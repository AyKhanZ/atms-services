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

    Task<bool> IsWorkTicketExistAsync(Guid projectId, Guid workTicketId, CancellationToken cancellationToken);

    Task<bool> IsProjectParticipantExistAsync(Guid projectId, Guid participantId, CancellationToken cancellationToken);

    Task<bool> IsStaffProjectParticipantExistAsync(Guid projectId, Guid participantId, CancellationToken cancellationToken);

    Task<WorkTaskProgress> GetProgressAsync(Guid parentWorkTaskId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, WorkTaskProgress>> GetProgressByTicketAsync(
        IReadOnlyCollection<Guid> workTicketIds,
        CancellationToken cancellationToken);

    Task CreateAsync(WorkTask workTask, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
