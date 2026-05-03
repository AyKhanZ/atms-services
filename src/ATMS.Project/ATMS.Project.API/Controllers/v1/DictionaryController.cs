using ATMS.Application.Models;
using ATMS.Project.Contracts.Requests.Dictionaries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Project.API.Controllers.v1;

[Authorize]
[Route("api/v1/dictionary")]
public class DictionaryController(IMediator mediator) : ControllerBase
{
    
    /// <summary>
    /// Gets all work project types .
    /// </summary>
    /// <remarks>
    /// Returns a list of available work project types dictionary values used in the system.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">Work project types</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("project-types")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetProjectTypes(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetProjectTypeDictionariesRequest(), cancellationToken));
    }
    
    /// <summary>
    /// Gets all work project kinds .
    /// </summary>
    /// <remarks>
    /// Returns a list of available work project kinds dictionary values used in the system.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">Work project kinds</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("project-kinds")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetProjectKinds(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetProjectKindDictionariesRequest(), cancellationToken));
    }
    
    /// <summary>
    /// Gets all work project statuses .
    /// </summary>
    /// <remarks>
    /// Returns a list of available work project statuses dictionary values used in the system.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">Work project statuses</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("project-statuses")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetProjectStatuses(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetProjectStatusDictionariesRequest(), cancellationToken));
    }
    
    
    
    /// <summary>
    /// Gets all work ticket statuses .
    /// </summary>
    /// <remarks>
    /// Returns a list of available work ticket statuses dictionary values used in the system.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">Work ticket statuses</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("work-ticket-statuses")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetWorkTicketStatuses(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetWorkTicketStatusDictionariesRequest(), cancellationToken));
    }
    
    /// <summary>
    /// Gets all work ticket types .
    /// </summary>
    /// <remarks>
    /// Returns a list of available work ticket types dictionary values used in the system.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">Work ticket types</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("work-ticket-types")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetWorkTicketTypes(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetWorkTicketTypeDictionariesRequest(), cancellationToken));
    }
    
    
    
    /// <summary>
    /// Gets all work task statuses .
    /// </summary>
    /// <remarks>
    /// Returns a list of available work task statuses dictionary values used in the system.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">Work task statuses</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("work-task-statuses")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetWorkTaskStatuses(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetWorkTaskStatusDictionariesRequest(), cancellationToken));
    }
    
    
    
    /// <summary>
    /// Gets all work item priorities .
    /// </summary>
    /// <remarks>
    /// Returns a list of available work item priorities dictionary values used in the system.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">work item priorities</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("work-item-priorities")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetWorkItemPriorities(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetWorkItemPriorityDictionariesRequest(), cancellationToken));
    }
    
    
    
    /// <summary>Gets all work group statuses</summary>
    /// <summary>
    /// Gets all work group statuses .
    /// </summary>
    /// <remarks>
    /// Returns a list of available work group statuses dictionary values used in the system.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <response code="200">work group statuses</response>
    /// <response code="401">Unauthorized access, no access token provided by a client</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("work-group-statuses")]
    [ProducesResponseType(typeof(DictionaryModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetWorkGroupStatuses(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetWorkGroupStatusDictionariesRequest(), cancellationToken));
    }
}