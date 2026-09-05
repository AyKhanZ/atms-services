using ATMS.Data.Criteria;
using ATMS.Data.Criteria.Users;
using ATMS.Data.Enums;
using ATMS.Project.Data.Criteria.Users;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkTasks;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IReadOnlyDictionary<Guid, WorkTaskProgress>> GetProgressByTicketAsync(IReadOnlyCollection<Guid> workTicketIds, CancellationToken cancellationToken)
    {
        if (workTicketIds.Count == 0)
        {
            return new Dictionary<Guid, WorkTaskProgress>();
        }

        return await context.WorkTasks
            .Where(task => workTicketIds.Contains(task.WorkTicketId) && task.ParentWorkTaskId == null)
            .GroupBy(task => task.WorkTicketId)
            .ToDictionaryAsync(
                group => group.Key,
                group => new WorkTaskProgress(
                    group.Count(),
                    group.Count(task => task.StatusId == (int)WorkTaskStatusEnum.Done)),
                cancellationToken);
    }

    public async Task CreateAsync(WorkTask workTask, CancellationToken cancellationToken)
    {
        await context.WorkTasks.AddAsync(workTask, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return context.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyDictionary<Guid, WorkTaskProgress>> GetProgressByParentAsync(IReadOnlyCollection<Guid> parentWorkTaskIds, CancellationToken cancellationToken)
    {
        if (parentWorkTaskIds.Count == 0)
        {
            return new Dictionary<Guid, WorkTaskProgress>();
        }

        return await context.WorkTasks
            .Where(task => task.ParentWorkTaskId.HasValue && parentWorkTaskIds.Contains(task.ParentWorkTaskId.Value))
            .GroupBy(task => task.ParentWorkTaskId!.Value)
            .ToDictionaryAsync(
                group => group.Key,
                group => new WorkTaskProgress(
                    group.Count(),
                    group.Count(task => task.StatusId == (int)WorkTaskStatusEnum.Done)),
                cancellationToken);
    }
}
