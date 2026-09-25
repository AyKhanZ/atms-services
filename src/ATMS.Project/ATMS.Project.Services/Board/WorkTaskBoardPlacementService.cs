using ATMS.Application.Exceptions.Conflict;
using ATMS.Data.Criteria.Interfaces;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Board.Interfaces;
using ATMS.Project.Services.Resources;

namespace ATMS.Project.Services.Board;

/// <summary>
/// Where a card goes in its New or In Progress column, and that it gets there. A place is unique in a
/// column, so a card saved a moment after another took the same place goes just below it instead of
/// failing. A gap between two cards that has no room left is made again by spreading the column's keys.
/// </summary>
public sealed class WorkTaskBoardPlacementService(
    IWorkTaskRepository workTaskRepository,
    IWorkTaskBoardPositionService positions) : IWorkTaskBoardPlacementService
{
    private const int Attempts = 3;

    public async Task PlaceOnTopAsync(WorkTask workTask, CancellationToken cancellationToken)
    {
        var top = await workTaskRepository.GetTopPlaceAsync(workTask.StatusId, cancellationToken);
        // Already the first card of this column: nothing to move. Compared by id, not by key: a card
        // coming from another column can carry the same key as the first card of this one.
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

            try
            {
                workTask.Rank = positions.Between(above, below);
                return;
            }
            catch (InvalidOperationException) when (!renumbered)
            {
                // The gap is used up after many drops into the same spot: spread the column once
                // and read the neighbours' new keys.
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

            // Another card took this key between the read and the save: go right below it.
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
