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
    /// Get Permissions
    /// </summary>
    /// <remarks>Method returns the list of current user permissions</remarks>
    /// <param name="id">User ID (Guid).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Array of items</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("{id:guid}/permissions")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string[]>> GetPermissionsAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCurrentPermissionsRequest { UserId = id }, cancellationToken);
        
        return Ok(result);
    }
    
    
    /// <summary>
    /// Get Roles 
    /// </summary>
    /// <remarks>Method returns the list of current user roles</remarks>
    /// <param name="id">User ID (Guid).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Dictionary of items</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("{id:guid}/roles")]
    [ProducesResponseType(typeof(DictionaryModel<Guid>[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DictionaryModel<Guid>[]>> GetRolesAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCurrentRolesRequest { UserId = id }, cancellationToken);
        
        return Ok(result);
    }
}
