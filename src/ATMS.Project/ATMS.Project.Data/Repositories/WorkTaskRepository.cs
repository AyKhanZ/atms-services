using ATMS.Data.Criteria.Interfaces;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Criteria.Users;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ATMS.Project.Data.Repositories;

public sealed class WorkTaskRepository(ProjectDbContext context) : IWorkTaskRepository
{
    public async Task<WorkTasksQueryResult> GetManyAsync(
        WorkTasksByProjectCriteria criteria,
        KeysetPaginationCriteria<WorkTask> pagination,
        CancellationToken cancellationToken)
    {
        var query = criteria.Apply(context.WorkTasks
            .AsNoTracking()
            .Include(task => task.Status)
                .ThenInclude(status => status.Translations)
            .Include(task => task.Priority)
                .ThenInclude(priority => priority.Translations)
            .Include(task => task.Assignee)
                .ThenInclude(participant => participant.User)
            .Include(task => task.WorkTicket)
                .ThenInclude(ticket => ticket.WorkGroup)
                .ThenInclude(milestone => milestone.ParentWorkGroup)
            .Include(task => task.ParentWorkTask)
            .AsSplitQuery());

        var items = await pagination
            .Apply(query, task => task.CreatedAt, task => task.Id)
            .ToArrayAsync(cancellationToken);

        var page = pagination.ToResult(items, task => task.CreatedAt, task => task.Id);
        var progress = await GetProgressByParentAsync(page.Items.Select(task => task.Id).ToArray(), cancellationToken);

        return new WorkTasksQueryResult(page, progress);
    }

    public Task<WorkTask?> GetAsync(Guid projectId, Guid workTaskId, CancellationToken cancellationToken)
    {
        return context.WorkTasks
            .AsNoTracking()
            .Include(task => task.Status)
                .ThenInclude(status => status.Translations)
            .Include(task => task.Priority)
                .ThenInclude(priority => priority.Translations)
            .Include(task => task.Assignee)
                .ThenInclude(participant => participant.User)
            .Include(task => task.WorkTicket)
                .ThenInclude(ticket => ticket.WorkGroup)
                .ThenInclude(milestone => milestone.ParentWorkGroup)
            .Include(task => task.ParentWorkTask)
            .Include(task => task.UpdatedBy)
            .AsSplitQuery()
            .FirstOrDefaultAsync(
                task => task.Id == workTaskId && task.WorkProjectId == projectId,
                cancellationToken);
    }

    public Task<WorkTask?> FindAsync(Guid projectId, Guid workTaskId, CancellationToken cancellationToken)
    {
        return context.WorkTasks.FirstOrDefaultAsync(
            task => task.Id == workTaskId && task.WorkProjectId == projectId,
            cancellationToken);
    }

    public Task<WorkTask?> FindParentAsync(Guid projectId, Guid parentWorkTaskId, CancellationToken cancellationToken)
    {
        return context.WorkTasks.FirstOrDefaultAsync(
            task => task.Id == parentWorkTaskId && task.WorkProjectId == projectId,
            cancellationToken);
    }

    public Task<bool> HasChildrenAsync(Guid projectId, Guid parentWorkTaskId, CancellationToken cancellationToken)
    {
        return context.WorkTasks.AnyAsync(
            task => task.WorkProjectId == projectId && task.ParentWorkTaskId == parentWorkTaskId,
            cancellationToken);
    }

    public Task<WorkTask[]> FindChildrenAsync(
        Guid projectId,
        Guid parentWorkTaskId,
        CancellationToken cancellationToken)
    {
        return context.WorkTasks
            .Where(task => task.WorkProjectId == projectId && task.ParentWorkTaskId == parentWorkTaskId)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Guid[]> GetIdsByTicketsAsync(IReadOnlyCollection<Guid> workTicketIds, CancellationToken cancellationToken)
    {
        if (workTicketIds.Count == 0)
        {
            return Task.FromResult(Array.Empty<Guid>());
        }

        return context.WorkTasks
            .Where(task => workTicketIds.Contains(task.WorkTicketId))
            .Select(task => task.Id)
            .ToArrayAsync(cancellationToken);
    }

    public Task<Guid[]> GetChildIdsAsync(Guid parentWorkTaskId, CancellationToken cancellationToken)
    {
        return context.WorkTasks
            .Where(task => task.ParentWorkTaskId == parentWorkTaskId)
            .Select(task => task.Id)
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> IsWorkTaskExistAsync(Guid projectId, Guid workTaskId, CancellationToken cancellationToken)
    {
        return context.WorkTasks.AnyAsync(
            task => task.Id == workTaskId && task.WorkProjectId == projectId,
            cancellationToken);
    }

    public Task<bool> IsWorkTaskExistAsync(Guid workTaskId, CancellationToken cancellationToken)
    {
        return context.WorkTasks.AnyAsync(task => task.Id == workTaskId, cancellationToken);
    }

    public Task<bool> IsWorkTicketExistAsync(Guid projectId, Guid workTicketId, CancellationToken cancellationToken)
    {
        return context.WorkTickets.AnyAsync(
            ticket => ticket.Id == workTicketId && ticket.WorkProjectId == projectId,
            cancellationToken);
    }

    public Task<bool> IsStaffProjectParticipantExistAsync(Guid projectId, Guid participantId, CancellationToken cancellationToken)
    {
        var employeeUsers = new EmployeeUsersCriteria().Apply(context.Users);

        return context.WorkProjectParticipants.AnyAsync(
            participant => participant.Id == participantId &&
                           participant.WorkProjectId == projectId &&
                           employeeUsers.Any(user => user.Id == participant.UserId),
            cancellationToken);
    }

    public Task<bool> CanBeAssignedAsync(Guid participantId, Guid? currentWorkTaskId, CancellationToken cancellationToken)
    {
        return context.WorkProjectParticipants.AnyAsync(
            participant => participant.Id == participantId &&
                           (participant.User.IsActive ||
                            context.WorkTasks.Any(item => item.Id == currentWorkTaskId && item.AssigneeId == participantId)),
            cancellationToken);
    }

    public Task<bool> IsProjectParticipantExistAsync(Guid projectId, Guid participantId, CancellationToken cancellationToken)
    {
        return context.WorkProjectParticipants.AnyAsync(
            participant => participant.Id == participantId && participant.WorkProjectId == projectId,
            cancellationToken);
    }

    public async Task<WorkTaskProgress> GetProgressAsync(Guid parentWorkTaskId, CancellationToken cancellationToken)
    {
        var progress = await context.WorkTasks
            .Where(task => task.ParentWorkTaskId == parentWorkTaskId)
            .GroupBy(task => task.ParentWorkTaskId)
            .Select(group => new WorkTaskProgress(
                group.Count(),
                group.Count(task => task.StatusId == (int)WorkTaskStatusEnum.Done)))
            .FirstOrDefaultAsync(cancellationToken);

        return progress ?? new WorkTaskProgress(0, 0);
    }

    public async Task<IReadOnlyDictionary<Guid, WorkTaskProgress>> GetProgressByTicketAsync(
        IReadOnlyCollection<Guid> workTicketIds,
        CancellationToken cancellationToken)
    {
        if (workTicketIds.Count == 0)
        {
            return new Dictionary<Guid, WorkTaskProgress>();
        }

        return await context.WorkTasks
            .Where(task => workTicketIds.Contains(task.WorkTicketId) && task.ParentWorkTaskId == null)
            .GroupBy(task => task.WorkTicketId)
            .Select(group => new
            {
                Id = group.Key,
                Total = group.Count(),
                Done = group.Count(task => task.StatusId == (int)WorkTaskStatusEnum.Done)
            })
            .ToDictionaryAsync(row => row.Id, row => new WorkTaskProgress(row.Total, row.Done), cancellationToken);
    }

    public Task<WorkTaskBoardPlace?> GetTopPlaceAsync(int statusId, CancellationToken cancellationToken)
    {
        return context.WorkTasks
            .Where(task => task.StatusId == statusId)
            .OrderBy(task => task.Rank)
            .Select(task => new WorkTaskBoardPlace(task.Id, task.Rank))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<string?> GetNextRankAsync(int statusId, string rank, CancellationToken cancellationToken)
    {
        return context.WorkTasks
            .Where(task => task.StatusId == statusId && string.Compare(task.Rank, rank) > 0)
            .OrderBy(task => task.Rank)
            .Select(task => task.Rank)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<string?> GetRankBelowAsync(
        int statusId,
        string? above,
        Guid exceptWorkTaskId,
        CancellationToken cancellationToken)
    {
        var column = context.WorkTasks
            .Where(task => task.StatusId == statusId && task.Id != exceptWorkTaskId);

        if (above is not null)
        {
            column = column.Where(task => string.Compare(task.Rank, above) > 0);
        }

        return column
            .OrderBy(task => task.Rank)
            .Select(task => task.Rank)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task RenumberColumnAsync(int statusId, CancellationToken cancellationToken)
    {
        // spread all keys evenly in 2 steps in one transaction: rank is unique per column, so one UPDATE could collide
        // '~' is never a key digit, so step 1 is safe; deleted cards are outside the unique index
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        await context.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE "Tasks" AS t
            SET "Rank" = '~' || lpad(ordered.n::text, 12, '0')
            FROM (
                SELECT "Id", row_number() OVER (ORDER BY "Rank", "Id") AS n
                FROM "Tasks"
                WHERE "StatusId" = {statusId} AND NOT "IsDeleted"
            ) AS ordered
            WHERE ordered."Id" = t."Id";
            """, cancellationToken);

        await context.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE "Tasks"
            SET "Rank" = 'm' || lpad((substr("Rank", 2)::bigint * 1000)::text, 12, '0') || 'v'
            WHERE "StatusId" = {statusId} AND NOT "IsDeleted" AND "Rank" LIKE '~%';
            """, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public Task<Dictionary<Guid, string>> GetRanksAsync(
        IReadOnlyCollection<Guid> workTaskIds,
        ICriteria<WorkTask> criteria,
        CancellationToken cancellationToken)
    {
        return criteria.Apply(context.WorkTasks)
            .Where(task => workTaskIds.Contains(task.Id))
            .ToDictionaryAsync(task => task.Id, task => task.Rank, cancellationToken);
    }

    public Task<WorkTask[]> FindByTicketAsync(Guid projectId, Guid workTicketId, CancellationToken cancellationToken)
    {
        return context.WorkTasks
            .Where(task => task.WorkProjectId == projectId && task.WorkTicketId == workTicketId)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAsync(WorkTask workTask, CancellationToken cancellationToken)
    {
        await context.WorkTasks.AddAsync(workTask, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: WorkTaskConfiguration.UniqueRankIndex
        })
        {
            // someone took this place a moment ago, the caller picks another
            return false;
        }
    }

    public async Task<IReadOnlyDictionary<Guid, WorkTaskProgress>> GetProgressByParentAsync(
        IReadOnlyCollection<Guid> parentWorkTaskIds,
        CancellationToken cancellationToken)
    {
        if (parentWorkTaskIds.Count == 0)
        {
            return new Dictionary<Guid, WorkTaskProgress>();
        }

        return await context.WorkTasks
            .Where(task => task.ParentWorkTaskId.HasValue && parentWorkTaskIds.Contains(task.ParentWorkTaskId.Value))
            .GroupBy(task => task.ParentWorkTaskId.Value)
            .Select(group => new
            {
                Id = group.Key,
                Total = group.Count(),
                Done = group.Count(task => task.StatusId == (int)WorkTaskStatusEnum.Done)
            })
            .ToDictionaryAsync(row => row.Id, row => new WorkTaskProgress(row.Total, row.Done), cancellationToken);
    }
}
