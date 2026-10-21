using ATMS.Application.Exceptions.Enums;
using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Infrastructure;
using ATMS.Project.Services.Domain.Board.Interfaces;
using ATMS.Project.Services.Domain.Notifications.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTasks;

public sealed class MoveWorkTaskHandler(
    IWorkTaskRepository workTaskRepository,
    ICacheService cache,
    ICurrentUser currentUser,
    IWorkTaskBoardPlacementService placement,
    IWorkTaskColumnService columns,
    IWorkTaskNotificationService notifications) : IRequestHandler<MoveWorkTaskCommand>
{
    public async Task Handle(MoveWorkTaskCommand command, CancellationToken cancellationToken)
    {
        var workTask = await workTaskRepository.FindAsync(command.ProjectId, command.WorkTaskId, cancellationToken)
            ?? throw new EntityException(EntityErrorTypeEnum.NotFound, WorkTaskMessages.NotFound);

        var now = DateTime.UtcNow;
        var previousStatusId = workTask.StatusId;
        var column = await columns.GetRequiredAsync(command.BoardColumnId, cancellationToken);
        var columnChanged = workTask.BoardColumnId != column.Id;
        columns.Assign(workTask, column, now);

        // Done is ordered by close date, no rank needed
        if (column.Category != (int)WorkTaskStatusEnum.Done)
        {
            var hasNeighbours = new[] { command.PreviousWorkTaskId, command.NextWorkTaskId }
                .Any(id => id.HasValue && id.Value != workTask.Id);

            if (hasNeighbours)
            {
                // neighbours only among the cards of that column the caller can see
                var neighbourCriteria = new WorkTaskBoardFilter { BoardColumnIds = [column.Id] }
                    .And(new ExceptSuperAdminCriteria<WorkTask>(
                        currentUser.RoleId,
                        new WorkTasksOfMyProjectsCriteria(currentUser.Id)));

                await placement.PlaceBetweenAsync(
                    workTask,
                    command.PreviousWorkTaskId,
                    command.NextWorkTaskId,
                    neighbourCriteria,
                    cancellationToken);
            }
            else if (columnChanged)
            {
                await placement.PlaceOnTopAsync(workTask, cancellationToken);
            }
        }

        var closedSubtasks = Array.Empty<Guid>();
        if (command.CompleteSubtasks && column.Category == (int)WorkTaskStatusEnum.Done)
        {
            var subtasks = await workTaskRepository.FindChildrenAsync(command.ProjectId, workTask.Id, cancellationToken);
            var open = subtasks.Where(subtask => subtask.StatusId != (int)WorkTaskStatusEnum.Done).ToArray();
            await columns.CloseOpenAsync(open, now, cancellationToken);
            closedSubtasks = open.Select(subtask => subtask.Id).ToArray();
        }

        await notifications.NotifyChangedAsync(workTask, workTask.AssigneeId, previousStatusId, cancellationToken);
        await placement.SaveAsync(workTask, cancellationToken);

        await cache.RemoveWorkTaskAsync(workTask.Id, cancellationToken);
        await cache.RemoveWorkTasksAsync(closedSubtasks, cancellationToken);

        if (workTask.ParentWorkTaskId.HasValue)
        {
            await cache.RemoveWorkTaskAsync(workTask.ParentWorkTaskId.Value, cancellationToken);
        }
    }
}
