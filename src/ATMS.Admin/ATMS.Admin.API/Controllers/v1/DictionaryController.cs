using ATMS.Admin.Contracts.Models.Dictionaries;
using ATMS.Admin.Contracts.Requests.Dictionaries;
using ATMS.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[Authorize]
[Route("api/v1/dictionary")]
public class DictionaryController(IMediator mediator) : ControllerBase
{
    
    /// <summary>
    /// Gets all genders .
    /// </summary>
    /// <remarks>
    /// Returns a list of available gender dictionary values used in the system.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">Genders</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("genders")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DictionaryModel[]>> GetGenders(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetGenderDictionariesRequest(), cancellationToken));
    }

    
    /// <summary>
    /// Gets all marital statuses .
    /// </summary>
    /// <remarks>
    /// Returns a list of marital status dictionary values.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">Marital statuses</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("marital-statuses")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DictionaryModel[]>> GetMaritalStatuses(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetMaritalStatusDictionariesRequest(), cancellationToken));
    }

    
    /// <summary>
    /// Gets all user statuses .
    /// </summary>
    /// <remarks>
    /// Returns all possible user statuses (e.g., Active, Inactive, Blocked).
    /// </remarks>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">User statuses</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("user-statuses")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<PermissionModel>>> GetUserStatuses(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetUserStatusDictionariesRequest(), cancellationToken));
    }
    
    
    /// <summary>
    /// Gets all roles .
    /// </summary>
    /// <remarks>
    /// Returns all system roles available for registration and role selection.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">Roles</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(DictionaryModel<Guid>[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DictionaryModel<Guid>[]>> GetRoles(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetRoleDictionariesRequest(), cancellationToken));
    }
    
    /// <summary>
    /// Gets all permissions .
    /// </summary>
    /// <remarks>
    /// Returns all system permissions available for role assignment.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">User statuses</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PermissionModel[]>> GetPermissions(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetPermissionDictionariesRequest(), cancellationToken));
    }
}