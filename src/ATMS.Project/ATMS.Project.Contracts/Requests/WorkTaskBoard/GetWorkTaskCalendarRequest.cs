using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.WorkTasks;
using MediatR;

namespace ATMS.Project.Contracts.Requests.WorkTaskBoard;

/// <summary>
/// The calendar's month at a glance: for every deadline in the range, how many tasks have it and the first few
/// of them. The rest of a day is read page by page from the task list.
/// </summary>
[Access(PermissionEnum.ProjectView)]
public sealed class GetWorkTaskCalendarRequest : IRequest<WorkTaskDeadlineGroupModel[]>
{
    /// <summary>Deadline on or after this moment: the client's local midnight of the first day on screen.</summary>
    public required DateTime DeadlineFrom { get; init; }

    /// <summary>Deadline before this moment. The range is at most 6 weeks, a month grid.</summary>
    public required DateTime DeadlineTo { get; init; }

    /// <summary>Filter by project. Omit to take every project the caller may see.</summary>
    public Guid[] ProjectIds { get; init; } = [];

    /// <summary>Filter by ticket. Use it with a single project.</summary>
    public Guid[] WorkTicketIds { get; init; } = [];

    /// <summary>Tasks (1), subtasks (2), or both when omitted.</summary>
    public int? Kind { get; init; }

    /// <summary>People by user id, not by participant: one person, many projects.</summary>
    public Guid[] AssigneeUserIds { get; init; } = [];

    /// <summary>Include tasks nobody is assigned to.</summary>
    public bool Unassigned { get; init; }

    /// <summary>Filter by task status.</summary>
    public int[] StatusIds { get; init; } = [];

    /// <summary>Filter by priority.</summary>
    public int[] PriorityIds { get; init; } = [];

    /// <summary>
    /// Only overdue work: not done, with a deadline before this moment. The client sends the start of
    /// its own today, since a deadline is the user's local midnight and the server does not know the zone.
    /// </summary>
    public DateTime? OverdueBefore { get; init; }

    /// <summary>Everything except overdue work: done, without a deadline, or due on or after this moment.</summary>
    public DateTime? ExcludeOverdueBefore { get; init; }

    /// <summary>Case-insensitive substring search by code or title.</summary>
    public string? Search { get; init; }
}
