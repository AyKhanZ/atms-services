using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Models;
using ATMS.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[Route("api/v1/auth")]
public class AuthenticationController(IMediator mediator) : ControllerBase
{

    /// <summary>
    /// Authenticates a user and issues access and refresh tokens.
    /// </summary>
    /// <remarks>
    /// User account must be active and not locked.
    /// On successful authentication, both access and refresh tokens are returned.
    /// </remarks>
    /// <param name="command">Login request containing user credentials.</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">Access info</response>
    /// <response code="400">Validation error, e.g., missing fields or invalid data.</response>
    /// <response code="401">The credentials are invalid or the email is not confirmed.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="423">Account temporary locked.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AccessInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string),StatusCodes.Status423Locked)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccessInfoModel>> LoginAsync(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    
    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// </summary>
    /// <remarks>
    /// The refresh token must be valid and not revoked.
    /// If the refresh token is expired or invalid, the request will be rejected.
    /// A new pair of access and refresh tokens is issued for the same session upon success.
    /// Refreshing one browser or device does not invalidate other active sessions.
    /// </remarks>
    /// <param name="command">Request containing refresh token.</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">Access info</response>
    /// <response code="400">Validation error, e.g., missing fields or invalid data.</response>
    /// <response code="401">The refresh token is invalid, expired, revoked, or was already used.</response>
    /// <response code="403">The user account is inactive.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AccessInfoModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccessInfoModel>> RefreshTokenAsync(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }
    
    
    /// <summary>
    /// Logs out a user by invalidating their tokens.
    /// </summary>
    /// <remarks>
    /// Only the session identified by the supplied refresh token is revoked.
    /// Other browsers and devices remain signed in. Repeating logout is safe.
    /// </remarks>
    /// <param name="command">Logout request containing the refresh token for one session.</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="204">No content, logout successful</response>
    /// <response code="400">Validation error, e.g., missing fields or invalid data.</response>
    /// <response code="500">Unhandled server error</response>
    [AllowAnonymous]
    [HttpDelete("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel),StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> LogoutAsync(
        [FromBody] LogoutCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        
        return NoContent();
    }
}
