using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Criteria.WorkTasks;

public class WorkTaskBoardFilter : ACriteria<WorkTask>
{
    private const int MaxSearchLength = 100;

    public IReadOnlyCollection<Guid> ProjectIds { get; init; } = [];

    public IReadOnlyCollection<Guid> WorkTicketIds { get; init; } = [];

    public WorkTaskKindEnum? Kind { get; init; }

    public IReadOnlyCollection<Guid> AssigneeUserIds { get; init; } = [];

    public bool Unassigned { get; init; }

    public IReadOnlyCollection<int> StatusIds { get; init; } = [];

    public IReadOnlyCollection<int> PriorityIds { get; init; } = [];

    public DateTime? DeadlineFrom { get; init; }

    public DateTime? DeadlineTo { get; init; }

    public bool NoDeadline { get; init; }

    public DateTime? OverdueBefore { get; init; }

    public string? Search { get; init; }

    public override IQueryable<WorkTask> Apply(IQueryable<WorkTask> query)
    {
        if (ProjectIds.Count > 0)
        {
            var projectIds = ProjectIds;
            query = query.Where(task => projectIds.Contains(task.WorkProjectId));
        }

        if (WorkTicketIds.Count > 0)
        {
            var ticketIds = WorkTicketIds;
            query = query.Where(task => ticketIds.Contains(task.WorkTicketId));
        }

        query = Kind switch
        {
            WorkTaskKindEnum.Task => query.Where(task => task.ParentWorkTaskId == null),
            WorkTaskKindEnum.Subtask => query.Where(task => task.ParentWorkTaskId != null),
            _ => query
        };

        if (AssigneeUserIds.Count > 0 || Unassigned)
        {
            var assignees = AssigneeUserIds;
            var unassigned = Unassigned;
            query = query.Where(task =>
                (unassigned && task.AssigneeId == null) ||
                (task.Assignee != null && assignees.Contains(task.Assignee.UserId)));
        }

        if (StatusIds.Count > 0)
        {
            var statusIds = StatusIds;
            query = query.Where(task => statusIds.Contains(task.StatusId));
        }

        if (PriorityIds.Count > 0)
        {
            var priorityIds = PriorityIds;
            query = query.Where(task => priorityIds.Contains(task.PriorityId));
        }

        if (NoDeadline)
        {
            query = query.Where(task => task.Deadline == null);
        }

        if (DeadlineFrom.HasValue)
        {
            var from = DeadlineFrom.Value;
            query = query.Where(task => task.Deadline >= from);
        }

        if (DeadlineTo.HasValue)
        {
            var to = DeadlineTo.Value;
            query = query.Where(task => task.Deadline < to);
        }

        if (OverdueBefore.HasValue)
        {
            var before = OverdueBefore.Value;
            query = query.Where(task =>
                task.Deadline < before && task.StatusId != (int)WorkTaskStatusEnum.Done);
        }

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var trimmed = Search.Trim();
            var term = trimmed[..Math.Min(trimmed.Length, MaxSearchLength)]
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
            var pattern = $"%{term}%";

            query = query.Where(task =>
                EF.Functions.ILike(task.Code, pattern, "\\") ||
                EF.Functions.ILike(task.Title, pattern, "\\"));
        }

        return query;
    }
}
