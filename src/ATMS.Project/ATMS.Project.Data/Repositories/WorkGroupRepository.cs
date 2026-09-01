using ATMS.Project.Data.DbContexts;
using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.WorkGroups;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public class WorkGroupRepository(ProjectDbContext context) : IWorkGroupRepository
{
    public async Task<KeysetPagedResult<WorkGroup>> GetMilestonesAsync(
        ACriteria<WorkGroup> criteria,
        KeysetPaginationCriteria<WorkGroup> pagination,
        CancellationToken cancellationToken)
    {
        var query = context.WorkGroups
            .Include(group => group.ParentWorkGroup)
            .AsNoTracking();
        query = criteria.Apply(query);

        var items = await pagination
            .Apply(query, milestone => milestone.CreatedAt, milestone => milestone.Id)
            .ToArrayAsync(cancellationToken);

        return pagination.ToResult(items, milestone => milestone.CreatedAt, milestone => milestone.Id);
    }

    public async Task<WorkGroupsQueryResult> GetGroupsAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var groups = await context.WorkGroups
            .Where(x => x.WorkProjectId == projectId && x.ParentWorkGroupId == null)
            .Include(x => x.Status)
                .ThenInclude(x => x.Translations)
            .Include(x => x.Children.OrderBy(child => child.CreatedAt))
                .ThenInclude(x => x.Status)
                .ThenInclude(x => x.Translations)
            .AsNoTracking()
            .AsSplitQuery()
            .OrderBy(x => x.CreatedAt)
            .ToArrayAsync(cancellationToken);

        var workGroupIds = groups
            .SelectMany(group => group.Children.Select(milestone => milestone.Id).Append(group.Id))
            .ToArray();

        if (workGroupIds.Length == 0)
        {
            return new WorkGroupsQueryResult(groups, new Dictionary<Guid, int>());
        }

        var ticketCounts = await context.WorkTickets
            .Where(ticket => workGroupIds.Contains(ticket.WorkGroupId))
            .GroupBy(ticket => ticket.WorkGroupId)
            .Select(group => new
            {
                WorkGroupId = group.Key,
                Count = group.Count()
            })
            .ToDictionaryAsync(
                item => item.WorkGroupId,
                item => item.Count,
                cancellationToken);

        return new WorkGroupsQueryResult(groups, ticketCounts);
    }

    public async Task<WorkGroup?> FindAsync(
        Guid projectId,
        Guid workGroupId,
        CancellationToken cancellationToken)
    {
        var workGroup = await context.WorkGroups.FindAsync(
            [workGroupId],
            cancellationToken);

        return workGroup is { IsDeleted: false } && workGroup.WorkProjectId == projectId
            ? workGroup
            : null;
    }

    public Task<bool> IsRootExistAsync(
        Guid projectId,
        Guid workGroupId,
        CancellationToken cancellationToken)
    {
        return context.WorkGroups.AnyAsync(
            x => x.Id == workGroupId &&
                 x.WorkProjectId == projectId &&
                 x.ParentWorkGroupId == null,
            cancellationToken);
    }

    public Task<bool> IsSiblingTitleExistAsync(
        Guid projectId,
        Guid? parentWorkGroupId,
        string normalizedTitle,
        Guid? excludedWorkGroupId,
        CancellationToken cancellationToken)
    {
        return context.WorkGroups.AnyAsync(
            x => x.WorkProjectId == projectId &&
                 x.ParentWorkGroupId == parentWorkGroupId &&
                 x.Title.Trim().ToLower() == normalizedTitle &&
                 (!excludedWorkGroupId.HasValue || x.Id != excludedWorkGroupId.Value),
            cancellationToken);
    }

    public Task<bool> HasChildrenAsync(Guid workGroupId, CancellationToken cancellationToken)
    {
        return context.WorkGroups.AnyAsync(
            x => x.ParentWorkGroupId == workGroupId,
            cancellationToken);
    }

    public Task<bool> HasTicketsAsync(Guid workGroupId, CancellationToken cancellationToken)
    {
        return context.WorkTickets.AnyAsync(
            x => x.WorkGroupId == workGroupId,
            cancellationToken);
    }

    public async Task CreateAsync(
        WorkGroup workGroup,
        CancellationToken cancellationToken)
    {
        await context.WorkGroups.AddAsync(workGroup, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
