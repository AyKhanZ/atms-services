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
            if (query.TryGetValue(nameof(request.Period), out var period) &&
                !int.TryParse(period.ToString(), out _))
            {
                throw new ValidationException(
                [new ValidationFailure(nameof(request.Period), DashboardMessages.PeriodUnsupported)]);
            }

            if (query.TryGetValue(nameof(request.ProjectId), out var rawProjectId) &&
                !string.IsNullOrWhiteSpace(rawProjectId.ToString()) &&
                !Guid.TryParse(rawProjectId.ToString(), out _))
            {
                throw new ValidationException(
                [new ValidationFailure(nameof(request.ProjectId), DashboardMessages.ProjectIdInvalid)]);
            }
        }

        if (request.Period is not (7 or 30 or 90))
        {
            throw new ValidationException(
            [new ValidationFailure(nameof(request.Period), DashboardMessages.PeriodUnsupported)]);
        }

        var accessibleProjects = new AccessibleWorkProjectsCriteria(currentUser.Id, currentUser.RoleId);
        if (request.ProjectId is { } projectId &&
            !await dashboardRepository.IsProjectAccessibleAsync(accessibleProjects, projectId, cancellationToken))
        {
            throw new EntityException(EntityErrorType.NotFound, WorkProjectMessages.NotFound);
        }

        var window = businessTimeZone.GetWindow(DateTime.UtcNow, request.Period);
        var isClient = currentUser.RoleId == RoleIds.Client || currentUser.RoleId == RoleIds.ClientManager;
        var data = await dashboardRepository.GetAsync(
            accessibleProjects,
            request.ProjectId,
            window.TodayStartUtc,
            window.PeriodStartUtc,
            window.PreviousStartUtc,
            window.PeriodEndUtc,
            window.DueEndUtc,
            window.OffsetHours,
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

        var days = Enumerable.Range(0, request.Period)
            .Select(offset => window.FirstDay.AddDays(offset))
            .ToArray();

        return new DashboardModel
        {
            GeneratedAt = window.GeneratedAt,
            Period = request.Period,
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
                new DashboardDoneKpiModel
                {
                    Key = "done",
                    Value = data.Done,
                    PreviousValue = data.PreviousDone,
                    ChangePercent = data.PreviousDone == 0
                        ? null
                        : (int)Math.Round(
                            (data.Done - data.PreviousDone) * 100d / data.PreviousDone,
                            MidpointRounding.AwayFromZero)
                }
            ],
            MainChart = new DashboardSeriesChartModel
            {
                Labels = days.Select(day => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).ToArray(),
                Series =
                [
                    new() { Key = "created", Data = days.Select(day => data.CreatedByDay.GetValueOrDefault(day.ToDateTime(TimeOnly.MinValue))).ToArray() },
                    new() { Key = "done", Data = days.Select(day => data.DoneByDay.GetValueOrDefault(day.ToDateTime(TimeOnly.MinValue))).ToArray() }
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
                    WorkTicketId = row.WorkTicketId,
                    WorkTaskId = row.Entry.EntityType == (int)HistoryEntityTypeEnum.WorkTask
                        ? row.Entry.EntityId : null
                },
                Entry = historyById[row.Entry.Id]
            }).ToArray()
        };
    }

    private static int Count(Dictionary<int, DashboardStatusCount> counts, WorkTaskStatusEnum status) =>
        counts.GetValueOrDefault((int)status)?.Count ?? 0;
}
