using ATMS.Admin.Contracts.Models.Me;
using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[Authorize]
[Route("api/v1/me")]
public class MeController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Gets the current user, including an account that is still completing onboarding.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The current user.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="404">The current user was not found.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(MeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MeModel>> GetMeAsync(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetMeRequest(), cancellationToken));
    }

    /// <summary>
    /// Gets the current user's permissions.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The permission codes.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="404">The current user was not found.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string[]>> GetPermissionsAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCurrentPermissionsRequest(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets the current user's roles.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The current user's roles.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="404">The current user was not found.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(DictionaryModel<Guid>[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DictionaryModel<Guid>[]>> GetRolesAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCurrentRolesRequest(), cancellationToken);
        return Ok(result);
    }
}
