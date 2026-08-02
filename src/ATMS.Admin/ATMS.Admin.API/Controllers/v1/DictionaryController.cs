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
    /// Gets all available languages.
    /// </summary>
    /// <remarks>
    /// Returns a list of available language dictionary values used in the system.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">The available languages.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpGet("languages")]
    [ProducesResponseType(typeof(LanguageModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LanguageModel[]>> GetLanguages(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetLanguageDictionariesRequest(), cancellationToken));
    }


    /// <summary>
    /// Gets all available genders.
    /// </summary>
    /// <remarks>
    /// Returns a list of available gender dictionary values used in the system.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">The available genders.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpGet("genders")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DictionaryModel[]>> GetGenders(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetGenderDictionariesRequest(), cancellationToken));
    }


    /// <summary>
    /// Gets all available marital statuses.
    /// </summary>
    /// <remarks>
    /// Returns a list of marital status dictionary values.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">The available marital statuses.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpGet("marital-statuses")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DictionaryModel[]>> GetMaritalStatuses(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetMaritalStatusDictionariesRequest(), cancellationToken));
    }

    
    /// <summary>
    /// Gets all available user statuses.
    /// </summary>
    /// <remarks>
    /// Returns all possible user statuses (e.g., Active, Inactive, Blocked).
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">The available user statuses.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="403">The authenticated user is not allowed to access this resource.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [Authorize]
    [HttpGet("user-statuses")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DictionaryModel[]>> GetUserStatuses(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetUserStatusDictionariesRequest(), cancellationToken));
    }
    
    
    /// <summary>
    /// Gets all available roles.
    /// </summary>
    /// <remarks>
    /// Returns all system roles available for registration and role selection.
    /// </remarks>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">The available roles.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="403">The authenticated user is not allowed to access this resource.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(DictionaryModel<Guid>[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <response code="200">The available permissions.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="403">The authenticated user is not allowed to access this resource.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PermissionModel[]>> GetPermissions(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetPermissionDictionariesRequest(), cancellationToken));
    }
}
