using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;
using ATMS.Application.Exceptions.Conflict;
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
    IWorkTaskBoardPositionService boardPositionService) : IRequestHandler<MoveWorkTaskCommand>
{
    public async Task Handle(MoveWorkTaskCommand command, CancellationToken cancellationToken)
    {
        var workTask = await workTaskRepository.FindAsync(command.ProjectId, command.WorkTaskId, cancellationToken)
            ?? throw new EntityException(EntityErrorType.NotFound, WorkTaskMessages.NotFound);

        var now = DateTime.UtcNow;
        var rank = await RankBetweenAsync(command, cancellationToken);
        if (rank is not null)
        {
            workTask.Rank = rank;
        }

        if (workTask.StatusId != command.StatusId)
        {
            workTask.StatusId = command.StatusId;
            workTask.DoneAt = command.StatusId == (int)WorkTaskStatusEnum.Done ? now : null;
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

        await workTaskRepository.SaveChangesAsync(cancellationToken);

        await cache.RemoveWorkTaskAsync(workTask.Id, cancellationToken);
        await cache.RemoveWorkTasksAsync(closedSubtasks, cancellationToken);

        if (workTask.ParentWorkTaskId.HasValue)
        {
            await cache.RemoveWorkTaskAsync(workTask.ParentWorkTaskId.Value, cancellationToken);
        }
    }

    private async Task<string?> RankBetweenAsync(MoveWorkTaskCommand command, CancellationToken cancellationToken)
    {
        if (command.StatusId == (int)WorkTaskStatusEnum.Done)
        {
            return null;
        }

        var neighbourIds = new[] { command.PreviousWorkTaskId, command.NextWorkTaskId }
            .Where(id => id.HasValue && id.Value != command.WorkTaskId)
            .Select(id => id!.Value)
            .ToArray();

        if (neighbourIds.Length == 0)
        {
            return null;
        }

        var criteria = new WorkTaskBoardFilter { StatusIds = [command.StatusId] }
            .And(new ExceptSuperAdminCriteria<WorkTask>(
                currentUser.RoleId,
                new WorkTasksOfMyProjectsCriteria(currentUser.Id)));
        var ranks = await workTaskRepository.GetRanksAsync(neighbourIds, criteria, cancellationToken);

        if (ranks.Count != neighbourIds.Distinct().Count())
        {
            throw new ConflictException(WorkTaskMessages.BoardPositionChanged);
        }

        var above = command.PreviousWorkTaskId is { } previous && ranks.TryGetValue(previous, out var a) ? a : null;
        var below = command.NextWorkTaskId is { } next && ranks.TryGetValue(next, out var b) ? b : null;

        if (above is not null && below is not null && string.CompareOrdinal(above, below) >= 0)
        {
            throw new ConflictException(WorkTaskMessages.BoardPositionChanged);
        }

        try
        {
            return above is null && below is null ? null : boardPositionService.Between(above, below);
        }
        catch (InvalidOperationException)
        {
            throw new ConflictException(WorkTaskMessages.BoardPositionUnavailable);
        }
    }
}
