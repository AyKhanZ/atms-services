using ATMS.Admin.Contracts.Models.Me;
using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[Authorize]
[Route("api/v1/me")]
public class MeController(IMediator mediator) : AdminControllerBase
{

    /// <summary>
    /// Get current user
    /// </summary>
    /// <remarks>Method returns the current user</remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Me model</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="404">User with specified ID not found.</response>
    /// <response code="500">Unhandled server error</response>
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
    /// Get Current User's Permissions
    /// </summary>
    /// <remarks>Method returns the list of current user permissions</remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Array of items</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="404">User with specified ID not found.</response>
    /// <response code="500">Unhandled server error</response>
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
    /// Get Current User's Roles 
    /// </summary>
    /// <remarks>Method returns the list of current user roles</remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Dictionary of items</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="404">User with specified ID not found.</response>
    /// <response code="500">Unhandled server error</response>
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
