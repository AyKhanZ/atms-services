using ATMS.Project.Data.Entities;

namespace ATMS.Project.Services.Domain.Board.Interfaces;

public interface IWorkTaskColumnService
{
    Task<BoardColumn> GetRequiredAsync(int id, CancellationToken cancellationToken);

    Task<BoardColumn> GetDefaultAsync(int statusId, CancellationToken cancellationToken);

    void Assign(WorkTask workTask, BoardColumn column, DateTime now);

    Task CloseOpenAsync(IEnumerable<WorkTask> workTasks, DateTime now, CancellationToken cancellationToken);
}
