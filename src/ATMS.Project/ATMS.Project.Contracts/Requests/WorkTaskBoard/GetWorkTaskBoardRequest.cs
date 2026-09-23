using ATMS.Application.Security;
using ATMS.Contracts.Requests;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.WorkTasks;
using MediatR;

namespace ATMS.Project.Contracts.Requests.WorkTaskBoard;

/// <summary>A page of tasks and subtasks from every project the caller may see.</summary>
[Access(PermissionEnum.ProjectView)]
public class GetWorkTaskBoardRequest : GetKeysetPaginationRequest, IRequest<KeysetPagedResult<WorkTaskModel>>
{
    /// <summary>Order by: board rank (1), close date (2), deadline (3), priority (4), title (5), state (6: New, In Progress, Done), numeric code (7).</summary>
    public int Sort { get; init; } = (int)WorkTaskBoardSortEnum.Rank;

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

    /// <summary>Deadline on or after this moment.</summary>
    public DateTime? DeadlineFrom { get; init; }

    /// <summary>Deadline before this moment.</summary>
    public DateTime? DeadlineTo { get; init; }

    /// <summary>Only tasks without a deadline.</summary>
    public bool NoDeadline { get; init; }

    /// <summary>
    /// Only overdue work: not done, with a deadline before this moment. The client sends the start of
    /// its own today, since a deadline is the user's local midnight and the server does not know the zone.
    /// </summary>
    public DateTime? OverdueBefore { get; init; }

    /// <summary>
    /// Everything except overdue work: done, without a deadline, or due on or after this moment. The
    /// board and the list read overdue work with <see cref="OverdueBefore"/> and the rest with this one,
    /// so overdue work stays on top whatever the order.
    /// </summary>
    public DateTime? ExcludeOverdueBefore { get; init; }

    /// <summary>Case-insensitive substring search by code or title.</summary>
    public string? Search { get; init; }
}
