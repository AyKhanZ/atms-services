using ATMS.Application.Models;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Commands.Search;
using ATMS.Project.Contracts.Models.Search;
using ATMS.Project.Contracts.Requests.Search;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Project.API.Controllers.v1;

[Authorize]
[Route("api/v1/search")]
public class SearchController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns projects, tickets, tasks and subtasks with a hasMore flag per group.
    /// </summary>
    /// <remarks>
    /// Matches an exact numeric code (with an optional #) or a case-insensitive title substring of at least three characters.
    /// Members only see their projects; super administrators see all projects that have not been deleted.
    /// Empty q returns the five most recently opened accessible items in recent; groups are empty.
    /// Results are not cached. take defaults to 5 and must be between 1 and 50.
    /// </remarks>
    /// <response code="200">Grouped search results, or recent items for an empty query.</response>
    /// <response code="400">The query or take is invalid.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="403">The user does not have project viewing permission.</response>
    /// <response code="500">The search could not be completed because of an unexpected server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(GlobalSearchModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GlobalSearchModel>> Get([FromQuery] GetGlobalSearchRequest request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    /// <summary>
    /// Lists one kind of search result, paged by cursor for infinite scrolling.
    /// </summary>
    /// <remarks>
    /// Backs the "show all" page. Same matching rules as the palette: an exact numeric code
    /// (with an optional #) or a case-insensitive title substring of at least three characters.
    /// Newest first by default; sortDirection reverses it. Pass the nextCursor of the previous
    /// response to continue. pageSize defaults to 20 and is capped at 50.
    /// </remarks>
    /// <param name="itemType">Project, Ticket, Task or Subtask, by name.</param>
    /// <param name="request">Query, cursor, page size and sort direction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">A page of results with a continuation token.</response>
    /// <response code="400">The query, item type, cursor or page size is invalid.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="403">The user does not have project viewing permission.</response>
    /// <response code="500">The search could not be completed because of an unexpected server error.</response>
    [HttpGet("{itemType}")]
    [ProducesResponseType(typeof(KeysetPagedResult<GlobalSearchItemModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<KeysetPagedResult<GlobalSearchItemModel>>> GetPage(
        GlobalSearchItemType itemType,
        [FromQuery] GetGlobalSearchPageRequest request,
        CancellationToken cancellationToken)
    {
        request.ItemType = itemType;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    /// <summary>
    /// Records an opened project, ticket, task or subtask in the current user's recent history.
    /// </summary>
    /// <remarks>
    /// Call after opening an item's page. Reopening updates its timestamp; only the last 20 items are retained.
    /// Item types are Project=1, Ticket=2, Task=3, Subtask=4. Access is checked again when recording and reading history;
    /// an item the user cannot see is silently not recorded.
    /// </remarks>
    /// <response code="204">The open was recorded.</response>
    /// <response code="400">The item identifier or type is invalid.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="403">The user does not have project viewing permission.</response>
    /// <response code="500">The open could not be recorded because of an unexpected server error.</response>
    [HttpPut("recent/{itemType}/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RecordRecent(int itemType, Guid itemId, CancellationToken cancellationToken)
    {
        await mediator.Send(new RecordGlobalSearchRecentCommand { ItemType = itemType, ItemId = itemId }, cancellationToken);
        return NoContent();
    }
}
