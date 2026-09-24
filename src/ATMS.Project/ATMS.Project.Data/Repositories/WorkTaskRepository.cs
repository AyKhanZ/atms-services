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

public class WorkTaskRepository(ProjectDbContext context) : IWorkTaskRepository
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

    public Task<string?> GetTopRankAsync(int statusId, CancellationToken cancellationToken)
    {
        return context.WorkTasks
            .Where(task => task.StatusId == statusId)
            .OrderBy(task => task.Rank)
            .Select(task => task.Rank)
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

    public async Task RenumberColumnAsync(int statusId, CancellationToken cancellationToken)
    {
        // Every key in the column spread evenly again, in the same order. Two steps inside one
        // transaction: the rank is unique within a column, and a single UPDATE could give one card
        // a key another card still holds. '~' is never a digit of a key, so the first step cannot
        // collide either. Deleted cards keep theirs: the unique index leaves them out.
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
            // Someone took this place in the column a moment earlier; the caller picks another.
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
