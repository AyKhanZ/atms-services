using ATMS.Application.Models;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.WorkTaskBoard;
using ATMS.Project.Contracts.Models.WorkTasks;
using ATMS.Project.Contracts.Requests.WorkTaskBoard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Project.API.Controllers.v1;

[Authorize]
[Route("api/v1/work-tasks")]
public class WorkTaskBoardController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns a cursor-paginated page of tasks and subtasks across the caller's projects.
    /// </summary>
    /// <remarks>
    /// Only projects the caller participates in are included; a super administrator sees all.
    /// Every list filter is "any of" and an empty one does not filter. sort=1 (Rank) is the board's
    /// own order, 2 (DoneAt) the Done column, 3 (Deadline) the calendar, 4 (Priority), 5 (Title)
    /// 6 (State: New, In Progress, Done) and 7 (numeric Code) are available for the list;
    /// sortDirection turns any of them round, and tasks without a deadline stay last either way.
    /// The cursor belongs to the sort and direction it was issued for.
    /// </remarks>
    /// <response code="200">Returns the page.</response>
    /// <response code="400">A filter, the sort, the page size or the cursor is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have permission to view projects.</response>
    [HttpGet]
    [ProducesResponseType(typeof(KeysetPagedResult<WorkTaskModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<KeysetPagedResult<WorkTaskModel>>> GetMany([FromQuery] GetWorkTaskBoardRequest request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    /// <summary>
    /// Returns how many tasks each status holds under the same filters.
    /// </summary>
    /// <remarks>
    /// Keys are status ids; statuses with no tasks are absent. Used for the board's column headers.
    /// </remarks>
    /// <response code="200">Returns the counts.</response>
    /// <response code="400">A filter is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have permission to view projects.</response>
    [HttpGet("counts")]
    [ProducesResponseType(typeof(Dictionary<int, int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Dictionary<int, int>>> GetCounts([FromQuery] GetWorkTaskBoardCountsRequest request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    /// <summary>
    /// Returns the people the "Assigned to" filter offers.
    /// </summary>
    /// <remarks>
    /// Staff of the given projects, or of all the caller's projects when none are given — one entry
    /// per person however many projects they are in.
    /// </remarks>
    /// <response code="200">Returns the people.</response>
    /// <response code="400">The project filter is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have permission to view projects.</response>
    [HttpGet("assignees")]
    [ProducesResponseType(typeof(WorkTaskBoardAssigneeModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WorkTaskBoardAssigneeModel[]>> GetAssignees([FromQuery] GetWorkTaskBoardAssigneesRequest request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }
}
