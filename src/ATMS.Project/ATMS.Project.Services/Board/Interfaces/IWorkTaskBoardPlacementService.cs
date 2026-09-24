using ATMS.Data.Criteria.Interfaces;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Services.Board.Interfaces;

public interface IWorkTaskBoardPlacementService
{
    Task PlaceOnTopAsync(WorkTask workTask, CancellationToken cancellationToken);

    Task PlaceBetweenAsync(
        WorkTask workTask,
        Guid? previousWorkTaskId,
        Guid? nextWorkTaskId,
        ICriteria<WorkTask> neighbourCriteria,
        CancellationToken cancellationToken);

    Task SaveAsync(WorkTask workTask, CancellationToken cancellationToken);
}
