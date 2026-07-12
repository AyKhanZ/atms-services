using ATMS.Admin.Contracts.Commands.Users;
using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Application.Models;
using ATMS.Data.Criteria;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[Authorize]
[Route("api/v1/users")]
public class UsersController(IMediator mediator) : ControllerBase
{
    
    /// <summary>
    /// Retrieves paginated and filtered list of users with optional filtering.
    /// </summary>
    /// <param name="request">Query parameters for filtering, paging, etc.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the list of users.</response>
    /// <response code="400">Invalid query parameters.</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserListItemModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<UserListItemModel>>> Index([FromQuery] GetUsersRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);

        return Ok(result);
    }

    
    /// <summary>
    /// Retrieves a single user by ID.
    /// </summary>
    /// <param name="id">User ID (Guid).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the user info.</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="404">User with specified ID not found.</response>
    /// <response code="500">Unhandled server error.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserModel>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserRequest { Id = id }, cancellationToken);

        return Ok(result);
    }
    
    
    /// <summary>
    /// Changes the user status.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <param name="command">Command containing user status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">User status successfully changed.</response>
    /// <response code="400">Validation error, e.g., password format invalid or missing fields.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="404">User with specified ID not found.</response>
    /// <response code="500">Unhandled server error.</response>
    [HttpPatch("status/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUserStatus(Guid id ,[FromBody] UpdateUserStatusCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await mediator.Send(command, cancellationToken);
        
        return NoContent();
    }
}