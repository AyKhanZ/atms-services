using ATMS.Admin.Contracts.Commands.UserProgresses;
using ATMS.Admin.Contracts.Models.UserProgresses;
using ATMS.Admin.Contracts.Requests.UserProgresses;
using ATMS.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[Authorize]
[Route("api/v1/user-progress")]
public class UserProgressController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves the current progress of the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the user progress.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(UserProgressModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserProgressModel>> GetProgress(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserProgressRequest(), cancellationToken);
        
        return Ok(result);
    }
    
        
    /// <summary>
    /// Updates the current progress of the user.
    /// </summary>
    /// <param name="command">Command containing progress data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Progress successfully updated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error.</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProgress([FromBody] UpdateUserProgressCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        
        return NoContent();
    }
    
    
    /// <summary>
    /// Submits the user progress for finalization.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Progress successfully submitted.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error.</response>
    [HttpPost("submit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SubmitProgress(CancellationToken cancellationToken)
    {
        await mediator.Send(new SubmitUserProgressCommand(), cancellationToken);

        return NoContent();
    }
}