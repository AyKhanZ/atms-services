using ATMS.Application.Models;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Models.History;
using ATMS.Project.Contracts.Requests.History;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Project.API.Controllers.v1;

[Authorize]
[Route("api/v1/project/{projectId:guid}/history")]
public class HistoryController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns who changed what and when, newest first.
    /// </summary>
    /// <remarks>
    /// Pass workTicketId for a ticket, workTaskId for a task or subtask, or neither for the project:
    /// its own changes together with the changes of its groups and milestones. Every change carries
    /// the old and the new value ready to show — statuses in the request's language, people with
    /// their names, tickets and tasks with their codes. Paged by cursor, 20 entries by default.
    /// </remarks>
    /// <response code="200">Returns one page of the history.</response>
    /// <response code="400">Both a ticket and a task were given, or the cursor or page size is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot view this project.</response>
    /// <response code="404">The project, ticket or task was not found.</response>
    [HttpGet]
    [ProducesResponseType(typeof(KeysetPagedResult<HistoryEntryModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<KeysetPagedResult<HistoryEntryModel>>> GetMany(
        Guid projectId,
        [FromQuery] GetHistoryRequest request,
        CancellationToken cancellationToken)
    {
        request.ProjectId = projectId;
        return Ok(await mediator.Send(request, cancellationToken));
    }


    /// <summary>
    /// Returns how the status changed, oldest first.
    /// </summary>
    /// <remarks>
    /// Same scope as the history: workTicketId, workTaskId or neither for the project. The first
    /// state has no date when the item was created before the history was kept. At most the latest
    /// 200 states are returned.
    /// </remarks>
    /// <response code="200">Returns the states.</response>
    /// <response code="400">Both a ticket and a task were given.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot view this project.</response>
    /// <response code="404">The project, ticket or task was not found.</response>
    [HttpGet("states")]
    [ProducesResponseType(typeof(IReadOnlyCollection<HistoryStateModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyCollection<HistoryStateModel>>> GetStates(
        Guid projectId,
        [FromQuery] GetHistoryStatesRequest request,
        CancellationToken cancellationToken)
    {
        request.ProjectId = projectId;
        return Ok(await mediator.Send(request, cancellationToken));
    }
}
