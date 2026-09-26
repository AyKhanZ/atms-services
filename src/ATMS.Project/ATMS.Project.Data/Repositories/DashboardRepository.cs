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
        DashboardDataWindow window,
        bool includeWorkload,
        CancellationToken cancellationToken)
    {
        var todayStartUtc = window.TodayStartUtc;
        var periodStartUtc = window.PeriodStartUtc;
        var previousStartUtc = window.PreviousStartUtc;
        var periodEndUtc = window.PeriodEndUtc;
        var dueEndUtc = window.DueEndUtc;
        var offsetHours = window.OffsetHours;

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

        var createdCounts = await tasks
            .Where(task => task.CreatedAt >= previousStartUtc && task.CreatedAt < periodEndUtc)
            .GroupBy(task => 1)
            .Select(group => new
            {
                Current = group.Count(task => task.CreatedAt >= periodStartUtc),
                Previous = group.Count(task => task.CreatedAt < periodStartUtc)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var createdByBucket = await CountByBucketAsync(
            tasks.Where(task => task.CreatedAt >= periodStartUtc && task.CreatedAt < periodEndUtc)
                .Select(task => task.CreatedAt),
            window,
            cancellationToken);

        // A task has no "started at" column: a move to In progress is known only from its history.
        // Every move counts, so a task taken back to work twice is started twice.
        var inProgress = ((int)WorkTaskStatusEnum.InProgress).ToString();
        var liveTaskIds = tasks.Select(task => task.Id);
        var startedByBucket = await CountByBucketAsync(
            context.HistoryEntries
                .AsNoTracking()
                .Where(entry => entry.EntityType == (int)HistoryEntityTypeEnum.WorkTask &&
                                entry.CreatedAt >= periodStartUtc && entry.CreatedAt < periodEndUtc &&
                                liveTaskIds.Contains(entry.EntityId) &&
                                entry.Changes.Any(change => change.Field == (int)HistoryFieldEnum.Status &&
                                                            change.NewValue == inProgress))
                .Select(entry => entry.CreatedAt),
            window,
            cancellationToken);

        var done = ((int)WorkTaskStatusEnum.Done).ToString();
        var doneByBucket = await CountByBucketAsync(
            context.HistoryEntries
                .AsNoTracking()
                .Where(entry => entry.EntityType == (int)HistoryEntityTypeEnum.WorkTask &&
                                entry.CreatedAt >= periodStartUtc && entry.CreatedAt < periodEndUtc &&
                                liveTaskIds.Contains(entry.EntityId) &&
                                entry.Changes.Any(change => change.Field == (int)HistoryFieldEnum.Status &&
                                                            change.NewValue == done))
                .Select(entry => entry.CreatedAt),
            window,
            cancellationToken);

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

        var upcoming = open.Where(task => task.Deadline >= todayStartUtc && task.Deadline < dueEndUtc);
        var deadlineCount = await upcoming.CountAsync(cancellationToken);
        var deadlines = await upcoming
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
            .Take(10)
            .Include(entry => entry.Changes)
            .ToArrayAsync(cancellationToken);
        var activityProjectIds = entries
            .Select(entry => entry.WorkProjectId)
            .Distinct()
            .ToArray();
        var taskIds = entries
            .Where(entry => entry.EntityType == (int)HistoryEntityTypeEnum.WorkTask)
            .Select(entry => entry.EntityId)
            .ToArray();
        var taskSubjects = taskIds.Length == 0
            ? new Dictionary<Guid, DashboardActivitySubjectRow>()
            : await context.WorkTasks
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(task => taskIds.Contains(task.Id) && activityProjectIds.Contains(task.WorkProjectId))
                .Select(task => new DashboardActivitySubjectRow(
                    task.Id,
                    "task",
                    task.Code,
                    task.Title,
                    task.IsDeleted,
                    task.WorkTicketId,
                    task.ParentWorkTaskId != null))
                .ToDictionaryAsync(subject => subject.Id, cancellationToken);
        var ticketIds = entries
            .Where(entry => entry.EntityType == (int)HistoryEntityTypeEnum.WorkTicket)
            .Select(entry => entry.EntityId)
            .ToArray();
        var ticketSubjects = ticketIds.Length == 0
            ? new Dictionary<Guid, DashboardActivitySubjectRow>()
            : await context.WorkTickets
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(ticket => ticketIds.Contains(ticket.Id) && activityProjectIds.Contains(ticket.WorkProjectId))
                .Select(ticket => new DashboardActivitySubjectRow(
                    ticket.Id, "ticket", ticket.Code, ticket.Title, ticket.IsDeleted, ticket.Id))
                .ToDictionaryAsync(subject => subject.Id, cancellationToken);
        var projectIds = entries
            .Where(entry => entry.EntityType != (int)HistoryEntityTypeEnum.WorkTask &&
                            entry.EntityType != (int)HistoryEntityTypeEnum.WorkTicket)
            .Select(entry => entry.WorkProjectId)
            .Distinct()
            .ToArray();
        var projectSubjects = projectIds.Length == 0
            ? new Dictionary<Guid, DashboardActivitySubjectRow>()
            : await context.WorkProjects
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(project => projectIds.Contains(project.Id))
                .Select(project => new DashboardActivitySubjectRow(
                    project.Id, "project", project.Code, project.Title, project.IsDeleted, null))
                .ToDictionaryAsync(subject => subject.Id, cancellationToken);
        // A history row can outlive its subject after a manual cleanup of the database; one such row
        // must drop out of the feed instead of failing the whole dashboard.
        var activities = new List<DashboardActivityRow>(entries.Length);
        foreach (var entry in entries)
        {
            var subject = entry.EntityType switch
            {
                (int)HistoryEntityTypeEnum.WorkTask => taskSubjects.GetValueOrDefault(entry.EntityId),
                (int)HistoryEntityTypeEnum.WorkTicket => ticketSubjects.GetValueOrDefault(entry.EntityId),
                _ => projectSubjects.GetValueOrDefault(entry.WorkProjectId)
            };

            if (subject is not null)
            {
                activities.Add(new DashboardActivityRow(entry, subject));
            }
        }

        return new DashboardData
        {
            StatusCounts = statusCounts,
            PriorityCounts = priorityCounts,
            Created = createdCounts?.Current ?? 0,
            PreviousCreated = createdCounts?.Previous ?? 0,
            Done = doneCounts?.Current ?? 0,
            PreviousDone = doneCounts?.Previous ?? 0,
            CreatedByBucket = createdByBucket,
            DoneByBucket = doneByBucket,
            StartedByBucket = startedByBucket,
            Workload = workload,
            Secondary = secondary,
            Deadlines = deadlines,
            DeadlineCount = deadlineCount,
            Activities = activities.ToArray()
        };
    }

    // Buckets are keyed by their start in business time: an hour of today, a day or the first day of
    // a month. The database groups; only the few bucket counts come back.
    private static async Task<Dictionary<DateTime, int>> CountByBucketAsync(
        IQueryable<DateTime> moments,
        DashboardDataWindow window,
        CancellationToken cancellationToken)
    {
        var local = moments.Select(moment => moment.AddHours(window.OffsetHours));

        return window.Granularity switch
        {
            DashboardGranularity.Hour => (await local
                    .GroupBy(moment => moment.Hour)
                    .Select(group => new { Hour = group.Key, Count = group.Count() })
                    .ToArrayAsync(cancellationToken))
                .ToDictionary(
                    row => window.TodayStartUtc.AddHours(window.OffsetHours).Date.AddHours(row.Hour),
                    row => row.Count),
            DashboardGranularity.Month => (await local
                    .GroupBy(moment => new { moment.Year, moment.Month })
                    .Select(group => new { group.Key.Year, group.Key.Month, Count = group.Count() })
                    .ToArrayAsync(cancellationToken))
                .ToDictionary(row => new DateTime(row.Year, row.Month, 1), row => row.Count),
            _ => await local
                .GroupBy(moment => moment.Date)
                .Select(group => new { Day = group.Key, Count = group.Count() })
                .ToDictionaryAsync(row => row.Day, row => row.Count, cancellationToken)
        };
    }
}
