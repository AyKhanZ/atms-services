using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Caching;
using ATMS.Project.Services.Board.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.WorkTasks;

public class MoveWorkTaskHandler(
    IWorkTaskRepository workTaskRepository,
    ICacheService cache,
    ICurrentUser currentUser,
    IWorkTaskBoardPlacementService placement) : IRequestHandler<MoveWorkTaskCommand>
{
    public async Task Handle(MoveWorkTaskCommand command, CancellationToken cancellationToken)
    {
        var workTask = await workTaskRepository.FindAsync(command.ProjectId, command.WorkTaskId, cancellationToken)
            ?? throw new EntityException(EntityErrorType.NotFound, WorkTaskMessages.NotFound);

        var now = DateTime.UtcNow;
        var statusChanged = workTask.StatusId != command.StatusId;
        if (statusChanged)
        {
            workTask.StatusId = command.StatusId;
            workTask.DoneAt = command.StatusId == (int)WorkTaskStatusEnum.Done ? now : null;
        }

        // Done keeps its own order, by close date: a card there needs no place of its own.
        if (command.StatusId != (int)WorkTaskStatusEnum.Done)
        {
            var hasNeighbours = new[] { command.PreviousWorkTaskId, command.NextWorkTaskId }
                .Any(id => id.HasValue && id.Value != workTask.Id);

            if (hasNeighbours)
            {
                // The neighbours are looked up among the cards of that column the caller may see.
                var neighbourCriteria = new WorkTaskBoardFilter { StatusIds = [command.StatusId] }
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
            else if (statusChanged)
            {
                await placement.PlaceOnTopAsync(workTask, cancellationToken);
            }
        }

        var closedSubtasks = Array.Empty<Guid>();
        if (command.CompleteSubtasks && command.StatusId == (int)WorkTaskStatusEnum.Done)
        {
            var subtasks = await workTaskRepository.FindChildrenAsync(command.ProjectId, workTask.Id, cancellationToken);
            var open = subtasks.Where(subtask => subtask.StatusId != (int)WorkTaskStatusEnum.Done).ToArray();

            foreach (var subtask in open)
            {
                subtask.StatusId = (int)WorkTaskStatusEnum.Done;
                subtask.DoneAt = now;
            }

            closedSubtasks = open.Select(subtask => subtask.Id).ToArray();
        }

        await placement.SaveAsync(workTask, cancellationToken);

        await cache.RemoveWorkTaskAsync(workTask.Id, cancellationToken);
        await cache.RemoveWorkTasksAsync(closedSubtasks, cancellationToken);

        if (workTask.ParentWorkTaskId.HasValue)
        {
            await cache.RemoveWorkTaskAsync(workTask.ParentWorkTaskId.Value, cancellationToken);
        }
    }
}
