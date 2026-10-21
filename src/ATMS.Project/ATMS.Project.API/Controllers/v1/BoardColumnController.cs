using ATMS.Application.Models;
using ATMS.Project.Contracts.Models.BoardColumns;
using ATMS.Project.Contracts.Requests.BoardColumns;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Project.API.Controllers.v1;

[Authorize]
[Route("api/v1/board-columns")]
public sealed class BoardColumnController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns the board columns in display order.
    /// </summary>
    /// <remarks>
    /// The columns are fixed and shared by the whole application: New; In Progress split into Doing
    /// and Done; Done. Each column belongs to one status (its category), and a task's status always
    /// equals the category of its column. The name and the label are in the caller's language.
    /// </remarks>
    /// <response code="200">Returns the columns.</response>
    /// <response code="401">The user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(typeof(BoardColumnModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BoardColumnModel[]>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetBoardColumnsRequest(), cancellationToken));
    }
}
