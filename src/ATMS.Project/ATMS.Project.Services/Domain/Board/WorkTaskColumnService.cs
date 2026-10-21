using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.Board.Interfaces;
using ATMS.Project.Services.Resources;

namespace ATMS.Project.Services.Domain.Board;

public sealed class WorkTaskColumnService(IBoardColumnRepository boardColumns) : IWorkTaskColumnService
{
    public async Task<BoardColumn> GetRequiredAsync(int id, CancellationToken cancellationToken)
    {
        return await boardColumns.FindAsync(id, cancellationToken)
            ?? throw new EntityException(EntityErrorTypeEnum.NotFound, WorkTaskMessages.ColumnUnsupported);
    }

    // the default column of a status carries the status's id (New, In Progress · Doing, Done)
    public Task<BoardColumn> GetDefaultAsync(int statusId, CancellationToken cancellationToken)
    {
        return GetRequiredAsync(statusId, cancellationToken);
    }

    public void Assign(WorkTask workTask, BoardColumn column, DateTime now)
    {
        var categoryChanged = workTask.StatusId != column.Category;
        workTask.BoardColumnId = column.Id;
        workTask.StatusId = column.Category;

        if (!categoryChanged)
        {
            return;
        }

        workTask.DoneAt = column.Category == (int)WorkTaskStatusEnum.Done ? now : null;
    }

    public async Task CloseOpenAsync(
        IEnumerable<WorkTask> workTasks,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var open = workTasks.Where(task => task.StatusId != (int)WorkTaskStatusEnum.Done).ToArray();
        if (open.Length == 0)
        {
            return;
        }

        var column = await GetDefaultAsync((int)WorkTaskStatusEnum.Done, cancellationToken);
        foreach (var task in open)
        {
            Assign(task, column, now);
        }
    }
}
