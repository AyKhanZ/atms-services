using ATMS.Application.Exceptions.Conflict;
using ATMS.Data.Criteria.Interfaces;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.Board.Interfaces;
using ATMS.Project.Services.Resources;

namespace ATMS.Project.Services.Domain.Board;

public sealed class WorkTaskBoardPlacementService(
    IWorkTaskRepository workTaskRepository,
    IWorkTaskBoardPositionService positions) : IWorkTaskBoardPlacementService
{
    private const int Attempts = 3;

    public async Task PlaceOnTopAsync(WorkTask workTask, CancellationToken cancellationToken)
    {
        var top = await workTaskRepository.GetTopPlaceAsync(workTask.StatusId, cancellationToken);
        // already first in this column; compare ids, a card from another column can have the same key
        if (top is not null && top.Id == workTask.Id)
        {
            return;
        }

        workTask.Rank = positions.Between(null, top?.Rank);
    }

    public async Task PlaceBetweenAsync(
        WorkTask workTask,
        Guid? previousWorkTaskId,
        Guid? nextWorkTaskId,
        ICriteria<WorkTask> neighbourCriteria,
        CancellationToken cancellationToken)
    {
        var neighbourIds = new[] { previousWorkTaskId, nextWorkTaskId }
            .Where(id => id.HasValue && id.Value != workTask.Id)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        for (var renumbered = false; ; renumbered = true)
        {
            var ranks = await workTaskRepository.GetRanksAsync(neighbourIds, neighbourCriteria, cancellationToken);
            if (ranks.Count != neighbourIds.Length)
            {
                throw new ConflictException(WorkTaskMessages.BoardPositionChanged);
            }

            var above = previousWorkTaskId is { } previous && ranks.TryGetValue(previous, out var a) ? a : null;
            var below = nextWorkTaskId is { } next && ranks.TryGetValue(next, out var b) ? b : null;
            if (above is not null && below is not null && string.CompareOrdinal(above, below) >= 0)
            {
                throw new ConflictException(WorkTaskMessages.BoardPositionChanged);
            }

            // the column holds cards of all projects, so a hidden card can sit right below; take the nearest key below
            var nearest = await workTaskRepository.GetRankBelowAsync(
                workTask.StatusId,
                above,
                workTask.Id,
                cancellationToken);
            if (nearest is not null && (below is null || string.CompareOrdinal(nearest, below) < 0))
            {
                below = nearest;
            }

            try
            {
                workTask.Rank = positions.Between(above, below);
                return;
            }
            catch (InvalidOperationException) when (!renumbered)
            {
                // the gap is used up after many drops into one spot, renumber the column once
                await workTaskRepository.RenumberColumnAsync(workTask.StatusId, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                throw new ConflictException(WorkTaskMessages.BoardPositionUnavailable);
            }
        }
    }

    public async Task SaveAsync(WorkTask workTask, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= Attempts; attempt++)
        {
            if (await workTaskRepository.TrySaveChangesAsync(cancellationToken))
            {
                return;
            }

            // another card took this key between read and save, go right below it
            var next = await workTaskRepository.GetNextRankAsync(workTask.StatusId, workTask.Rank, cancellationToken);
            try
            {
                workTask.Rank = positions.Between(workTask.Rank, next);
            }
            catch (InvalidOperationException)
            {
                break;
            }
        }

        throw new ConflictException(WorkTaskMessages.BoardPositionChanged);
    }
}
