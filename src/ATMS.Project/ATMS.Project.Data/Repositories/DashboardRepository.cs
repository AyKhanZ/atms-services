using ATMS.Data.Criteria.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Data.Criteria.WorkTasks;
using ATMS.Project.Data.DbContexts;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Dashboard;
using ATMS.Project.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Repositories;

public sealed class DashboardRepository(ProjectDbContext context) : IDashboardRepository
{
    public Task<bool> IsProjectAccessibleAsync(
        ICriteria<WorkProject> accessibleProjects,
        Guid projectId,
        CancellationToken cancellationToken) =>
        accessibleProjects.Apply(context.WorkProjects.AsNoTracking())
            .AnyAsync(project => project.Id == projectId, cancellationToken);

    public async Task<DashboardData> GetAsync(
        ICriteria<WorkProject> accessibleProjects,
        Guid? projectId,
        DateTime todayStartUtc,
        DateTime periodStartUtc,
        DateTime previousStartUtc,
        DateTime periodEndUtc,
        DateTime dueEndUtc,
        double offsetHours,
        bool includeWorkload,
        CancellationToken cancellationToken)
    {
        var visibleProjectIds = accessibleProjects
            .Apply(context.WorkProjects.AsNoTracking())
            .Select(project => project.Id);
        var tasks = new WorkTasksOfLiveWorkCriteria().Apply(context.WorkTasks
            .AsNoTracking()
            .Where(task => visibleProjectIds.Contains(task.WorkProjectId)));

        if (projectId.HasValue)
        {
            tasks = tasks.Where(task => task.WorkProjectId == projectId.Value);
        }

        var open = tasks.Where(task => task.StatusId != (int)WorkTaskStatusEnum.Done);

        var statusRows = await tasks
            .GroupBy(task => task.StatusId)
            .Select(group => new
            {
                Id = group.Key,
                Count = group.Count(),
                Unassigned = group.Count(task => task.AssigneeId == null),
                Overdue = group.Count(task => task.Deadline < todayStartUtc)
            })
            .ToArrayAsync(cancellationToken);
        var statusCounts = statusRows.ToDictionary(
            row => row.Id,
            row => new DashboardStatusCount(row.Count, row.Unassigned, row.Overdue));

        var priorityCounts = await open
            .GroupBy(task => task.PriorityId)
            .Select(group => new { Id = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.Id, row => row.Count, cancellationToken);

        var doneCounts = await tasks
            .Where(task => task.StatusId == (int)WorkTaskStatusEnum.Done &&
                           task.DoneAt >= previousStartUtc && task.DoneAt < periodEndUtc)
            .GroupBy(task => 1)
            .Select(group => new
            {
                Current = group.Count(task => task.DoneAt >= periodStartUtc),
                Previous = group.Count(task => task.DoneAt < periodStartUtc)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var createdByDay = await tasks
            .Where(task => task.CreatedAt >= periodStartUtc && task.CreatedAt < periodEndUtc)
            .GroupBy(task => task.CreatedAt.AddHours(offsetHours).Date)
            .Select(group => new { Day = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.Day, row => row.Count, cancellationToken);

        var doneByDay = await tasks
            .Where(task => task.StatusId == (int)WorkTaskStatusEnum.Done &&
                           task.DoneAt >= periodStartUtc && task.DoneAt < periodEndUtc)
            .GroupBy(task => task.DoneAt!.Value.AddHours(offsetHours).Date)
            .Select(group => new { Day = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.Day, row => row.Count, cancellationToken);

        DashboardWorkloadRow[] workload = [];
        if (includeWorkload)
        {
            var workloadRows = await open
                .Where(task => task.AssigneeId != null)
                .GroupBy(task => new
                {
                    task.Assignee!.UserId,
                    task.Assignee.User.Name,
                    task.Assignee.User.Surname,
                    task.Assignee.User.AvatarPath
                })
                .Select(group => new
                {
                    group.Key.UserId,
                    group.Key.Name,
                    group.Key.Surname,
                    group.Key.AvatarPath,
                    Count = group.Count()
                })
                .OrderByDescending(row => row.Count)
                .ThenBy(row => row.Name)
                .ThenBy(row => row.Surname)
                .Take(8)
                .ToArrayAsync(cancellationToken);
            workload = workloadRows
                .Select(row => new DashboardWorkloadRow
                {
                    Id = row.UserId,
                    Name = row.Name,
                    Surname = row.Surname,
                    AvatarPath = row.AvatarPath,
                    Count = row.Count
                })
                .ToArray();
        }

        DashboardEntityRow[] secondary;
        if (projectId.HasValue)
        {
            var secondaryRows = await open
                .GroupBy(task => new { task.WorkTicketId, task.WorkTicket.Code, task.WorkTicket.Title })
                .Select(group => new
                {
                    Id = group.Key.WorkTicketId,
                    group.Key.Code,
                    group.Key.Title,
                    Count = group.Count()
                })
                .OrderByDescending(row => row.Count)
                .ThenBy(row => row.Id)
                .Take(8)
                .ToArrayAsync(cancellationToken);
            secondary = secondaryRows
                .Select(row => new DashboardEntityRow
                {
                    Id = row.Id,
                    Code = row.Code,
                    Name = row.Title,
                    Count = row.Count
                })
                .ToArray();
        }
        else
        {
            var secondaryRows = await open
                .GroupBy(task => new { task.WorkProjectId, task.WorkProject.Code, task.WorkProject.Title })
                .Select(group => new
                {
                    Id = group.Key.WorkProjectId,
                    group.Key.Code,
                    group.Key.Title,
                    Count = group.Count()
                })
                .OrderByDescending(row => row.Count)
                .ThenBy(row => row.Id)
                .Take(8)
                .ToArrayAsync(cancellationToken);
            secondary = secondaryRows
                .Select(row => new DashboardEntityRow
                {
                    Id = row.Id,
                    Code = row.Code,
                    Name = row.Title,
                    Count = row.Count
                })
                .ToArray();
        }

        var deadlines = await open
            .Where(task => task.Deadline >= todayStartUtc && task.Deadline < dueEndUtc)
            .OrderBy(task => task.Deadline)
            .ThenBy(task => task.Id)
            .Take(10)
            .Select(task => new DashboardDeadlineRow(
                task.Id,
                task.WorkProjectId,
                task.WorkTicketId,
                task.Code,
                task.Title,
                task.ParentWorkTaskId != null,
                task.Deadline!.Value,
                task.PriorityId,
                task.Assignee == null ? null : task.Assignee.UserId,
                task.Assignee == null ? null : task.Assignee.User.Name,
                task.Assignee == null ? null : task.Assignee.User.Surname,
                task.Assignee == null ? null : task.Assignee.User.AvatarPath))
            .ToArrayAsync(cancellationToken);

        var history = context.HistoryEntries
            .AsNoTracking()
            .Where(entry => visibleProjectIds.Contains(entry.WorkProjectId));
        if (projectId.HasValue)
        {
            history = history.Where(entry => entry.WorkProjectId == projectId.Value);
        }

        var entries = await history
            .OrderByDescending(entry => entry.CreatedAt)
            .ThenByDescending(entry => entry.Id)
            .Take(15)
            .Include(entry => entry.Changes)
            .ToArrayAsync(cancellationToken);
        var taskIds = entries
            .Where(entry => entry.EntityType == (int)HistoryEntityTypeEnum.WorkTask)
            .Select(entry => entry.EntityId)
            .ToArray();
        var taskTickets = taskIds.Length == 0
            ? new Dictionary<Guid, Guid>()
            : await context.WorkTasks
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(task => taskIds.Contains(task.Id))
                .Select(task => new { task.Id, task.WorkTicketId })
                .ToDictionaryAsync(task => task.Id, task => task.WorkTicketId, cancellationToken);
        var activities = entries
            .Select(entry => new DashboardActivityRow(
                entry,
                entry.EntityType switch
                {
                    (int)HistoryEntityTypeEnum.WorkTask =>
                        taskTickets.TryGetValue(entry.EntityId, out var ticketId) ? ticketId : null,
                    (int)HistoryEntityTypeEnum.WorkTicket => entry.EntityId,
                    _ => null
                }))
            .ToArray();

        return new DashboardData
        {
            StatusCounts = statusCounts,
            PriorityCounts = priorityCounts,
            Done = doneCounts?.Current ?? 0,
            PreviousDone = doneCounts?.Previous ?? 0,
            CreatedByDay = createdByDay,
            DoneByDay = doneByDay,
            Workload = workload,
            Secondary = secondary,
            Deadlines = deadlines,
            Activities = activities
        };
    }
}
