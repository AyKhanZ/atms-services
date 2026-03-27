using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[Authorize]
[Route("api/v1/users")]
public class UsersController(IMediator mediator) : AdminControllerBase
{
    
    /// <summary>
    /// Retrieves a list of users with optional filtering.
    /// </summary>
    /// <param name="request">Query parameters for filtering, paging, etc.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the list of users.</response>
    /// <response code="400">Invalid query parameters.</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(UserModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserModel[]>> Index([FromQuery] GetUsersRequest request, CancellationToken cancellationToken)
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
}
