using System.Globalization;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Dashboard;
using ATMS.Project.Contracts.Models.History;
using ATMS.Project.Contracts.Requests.Dashboard;
using ATMS.Project.Data.Criteria.WorkProjects;
using ATMS.Project.Data.Models.Dashboard;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Dashboard;
using ATMS.Project.Services.Dictionaries.Interfaces;
using ATMS.Project.Services.History.Interfaces;
using ATMS.Project.Services.Resources;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ATMS.Project.Services.Handlers.Dashboard;

public sealed class GetDashboardHandler(
    ICurrentUser currentUser,
    IHttpContextAccessor httpContextAccessor,
    BusinessTimeZone businessTimeZone,
    IDashboardRepository dashboardRepository,
    IDictionaryCacheService dictionaries,
    IHistoryValueResolver historyValueResolver)
    : IRequestHandler<GetDashboardRequest, DashboardModel>
{
    public async Task<DashboardModel> Handle(GetDashboardRequest request, CancellationToken cancellationToken)
    {
        var query = httpContextAccessor.HttpContext?.Request.Query;
        if (query is not null)
        {
            // A malformed date never reaches the request: model binding drops it, and the range would
            // then read as "pick both dates" instead of what actually went wrong.
            foreach (var field in new[] { nameof(request.From), nameof(request.To) })
            {
                if (query.TryGetValue(field, out var rawDate) &&
                    !string.IsNullOrWhiteSpace(rawDate.ToString()) &&
                    !DateOnly.TryParseExact(
                        rawDate.ToString(),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out _))
                {
                    throw new ValidationException([new ValidationFailure(field, DashboardMessages.DateInvalid)]);
                }
            }

            if (query.TryGetValue(nameof(request.ProjectId), out var rawProjectId) &&
                !string.IsNullOrWhiteSpace(rawProjectId.ToString()) &&
                !Guid.TryParse(rawProjectId.ToString(), out _))
            {
                throw new ValidationException(
                [new ValidationFailure(nameof(request.ProjectId), DashboardMessages.ProjectIdInvalid)]);
            }
        }

        var window = businessTimeZone.GetWindow(DateTime.UtcNow, request.Period, request.From, request.To);

        var accessibleProjects = new AccessibleWorkProjectsCriteria(currentUser.Id, currentUser.RoleId);
        if (request.ProjectId is { } projectId &&
            !await dashboardRepository.IsProjectAccessibleAsync(accessibleProjects, projectId, cancellationToken))
        {
            throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.NotFound);
        }

        var isClient = currentUser.RoleId == RoleIds.Client || currentUser.RoleId == RoleIds.ClientManager;
        var data = await dashboardRepository.GetAsync(
            accessibleProjects,
            request.ProjectId,
            window.Data,
            !isClient,
            cancellationToken);
        var statuses = await dictionaries.GetWorkTaskStatusesAsync(cancellationToken);
        var priorities = await dictionaries.GetWorkItemPrioritiesAsync(cancellationToken);
        var priorityNames = priorities.ToDictionary(item => item.Id, item => item.Name);
        var historyEntries = await historyValueResolver.ResolveEntriesAsync(
            data.Activities.Select(activity => activity.Entry).ToArray(), cancellationToken);
        var historyById = historyEntries.ToDictionary(entry => entry.Id);

        var newCount = Count(data.StatusCounts, WorkTaskStatusEnum.New);
        var inProgressCount = Count(data.StatusCounts, WorkTaskStatusEnum.InProgress);
        var openCount = newCount + inProgressCount;
        var unassigned = data.StatusCounts.GetValueOrDefault((int)WorkTaskStatusEnum.New)?.Unassigned ?? 0;
        unassigned += data.StatusCounts.GetValueOrDefault((int)WorkTaskStatusEnum.InProgress)?.Unassigned ?? 0;
        var others = openCount - unassigned - data.Workload.Sum(row => row.Count);

        var buckets = Buckets(window);
        var labelFormat = window.Data.Granularity switch
        {
            DashboardGranularity.Hour => "yyyy-MM-ddTHH:mm",
            DashboardGranularity.Month => "yyyy-MM",
            _ => "yyyy-MM-dd"
        };

        return new DashboardModel
        {
            GeneratedAt = window.GeneratedAt,
            Period = window.Period,
            From = window.FirstDay,
            To = window.LastDay,
            Granularity = window.Data.Granularity.ToString().ToLowerInvariant(),
            Kpis =
            [
                new() { Key = "open", Value = openCount },
                new() { Key = "inProgress", Value = inProgressCount },
                new()
                {
                    Key = "overdue",
                    Value = (data.StatusCounts.GetValueOrDefault((int)WorkTaskStatusEnum.New)?.Overdue ?? 0) +
                            (data.StatusCounts.GetValueOrDefault((int)WorkTaskStatusEnum.InProgress)?.Overdue ?? 0)
                },
                new() { Key = "unassigned", Value = unassigned },
                Trend("created", data.Created, data.PreviousCreated),
                Trend("done", data.Done, data.PreviousDone)
            ],
            MainChart = new DashboardSeriesChartModel
            {
                Labels = buckets
                    .Select(bucket => bucket.ToString(labelFormat, CultureInfo.InvariantCulture))
                    .ToArray(),
                Series =
                [
                    new()
                    {
                        Key = "created",
                        Data = buckets.Select(bucket => data.CreatedByBucket.GetValueOrDefault(bucket)).ToArray()
                    },
                    new()
                    {
                        Key = "started",
                        Data = buckets.Select(bucket => data.StartedByBucket.GetValueOrDefault(bucket)).ToArray()
                    },
                    new()
                    {
                        Key = "done",
                        Data = buckets.Select(bucket => data.DoneByBucket.GetValueOrDefault(bucket)).ToArray()
                    }
                ]
            },
            Donuts =
            [
                new()
                {
                    Key = "byStatus",
                    Segments = statuses.OrderBy(item => item.Id)
                        .Select(item => new DashboardDictionarySegmentModel
                        {
                            Id = item.Id,
                            Label = item.Name,
                            Value = data.StatusCounts.GetValueOrDefault(item.Id)?.Count ?? 0
                        })
                        .ToArray()
                },
                new()
                {
                    Key = "byPriority",
                    Segments = priorities.OrderBy(item => item.Id)
                        .Select(item => new DashboardDictionarySegmentModel
                        {
                            Id = item.Id,
                            Label = item.Name,
                            Value = data.PriorityCounts.GetValueOrDefault(item.Id)
                        })
                        .ToArray()
                }
            ],
            Workload = isClient ? null : new DashboardWorkloadModel
            {
                Segments = data.Workload
                    .Select(row => new DashboardWorkloadSegmentModel
                    {
                        Kind = "user",
                        Person = new HistoryPersonModel
                        {
                            Id = row.Id,
                            Name = row.Name,
                            Surname = row.Surname,
                            AvatarPath = row.AvatarPath
                        },
                        Value = row.Count
                    })
                    .Concat(
                    [
                        new DashboardWorkloadSegmentModel { Kind = "others", Person = null, Value = others },
                        new DashboardWorkloadSegmentModel { Kind = "unassigned", Person = null, Value = unassigned }
                    ])
                    .ToArray()
            },
            SecondaryChart = new DashboardSecondaryChartModel
            {
                Key = request.ProjectId.HasValue ? "byTicket" : "byProject",
                Segments = data.Secondary.Select(row => new DashboardEntitySegmentModel
                {
                    Id = row.Id,
                    Code = row.Code,
                    Label = row.Name,
                    Value = row.Count
                }).ToArray()
            },
            DeadlineCount = data.DeadlineCount,
            Deadlines = data.Deadlines.Select(row => new DashboardDeadlineModel
            {
                Ref = new DashboardRefModel
                {
                    ProjectId = row.WorkProjectId,
                    WorkTicketId = row.WorkTicketId,
                    WorkTaskId = row.Id
                },
                Code = row.Code,
                Title = row.Title,
                IsSubtask = row.IsSubtask,
                Deadline = row.Deadline,
                Priority = new DashboardPriorityModel
                {
                    Id = row.PriorityId,
                    Name = priorityNames[row.PriorityId]
                },
                Assignee = row.AssigneeUserId is { } userId
                    ? new HistoryPersonModel
                    {
                        Id = userId,
                        Name = row.AssigneeName ?? throw new InvalidOperationException("Assignee name is missing."),
                        Surname = row.AssigneeSurname ?? throw new InvalidOperationException("Assignee surname is missing."),
                        AvatarPath = row.AssigneeAvatarPath
                    }
                    : null
            }).ToArray(),
            Activities = data.Activities.Select(row => new DashboardActivityModel
            {
                Ref = new DashboardRefModel
                {
                    ProjectId = row.Entry.WorkProjectId,
                    WorkTicketId = row.Subject.WorkTicketId,
                    WorkTaskId = row.Entry.EntityType == (int)HistoryEntityTypeEnum.WorkTask
                        ? row.Entry.EntityId : null
                },
                Subject = new DashboardActivitySubjectModel
                {
                    Type = row.Subject.Type,
                    Code = row.Subject.Code,
                    Title = row.Subject.Title,
                    IsDeleted = row.Subject.IsDeleted,
                    IsSubtask = row.Subject.IsSubtask
                },
                Entry = historyById[row.Entry.Id]
            }).ToArray()
        };
    }

    private static DashboardTrendKpiModel Trend(string key, int value, int previousValue) => new()
    {
        Key = key,
        Value = value,
        PreviousValue = previousValue,
        ChangePercent = previousValue == 0
            ? null
            : (int)Math.Round((value - previousValue) * 100d / previousValue, MidpointRounding.AwayFromZero)
    };

    // Every bucket of the period is present, empty ones as zero, so the line never breaks.
    private static DateTime[] Buckets(DashboardPeriodWindow window)
    {
        var first = window.FirstDay.ToDateTime(TimeOnly.MinValue);
        var monthStart = new DateTime(window.FirstDay.Year, window.FirstDay.Month, 1);
        var months = (window.LastDay.Year - window.FirstDay.Year) * 12 +
                     window.LastDay.Month - window.FirstDay.Month + 1;

        return window.Data.Granularity switch
        {
            DashboardGranularity.Hour => Enumerable.Range(0, 24).Select(hour => first.AddHours(hour)).ToArray(),
            DashboardGranularity.Month => Enumerable.Range(0, months).Select(monthStart.AddMonths).ToArray(),
            _ => Enumerable.Range(0, window.LastDay.DayNumber - window.FirstDay.DayNumber + 1)
                .Select(offset => first.AddDays(offset))
                .ToArray()
        };
    }

    private static int Count(Dictionary<int, DashboardStatusCount> counts, WorkTaskStatusEnum status) =>
        counts.GetValueOrDefault((int)status)?.Count ?? 0;
}
