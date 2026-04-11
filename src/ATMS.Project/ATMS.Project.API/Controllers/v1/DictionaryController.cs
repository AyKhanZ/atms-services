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
    
    /// <summary>Gets all work project types</summary>
    [HttpGet("project-types")]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetProjectTypes(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetProjectTypeDictionariesRequest(), cancellationToken));
    }
    
    /// <summary>Gets all work project kinds</summary>
    [HttpGet("project-kinds")]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetProjectKinds(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetProjectKindDictionariesRequest(), cancellationToken));
    }
    
    /// <summary>Gets all work project statuses</summary>
    [HttpGet("project-statuses")]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetProjectStatuses(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetProjectStatusDictionariesRequest(), cancellationToken));
    }
    
    
    
    /// <summary>Gets all work ticket statuses</summary>
    [HttpGet("work-ticket-statuses")]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetWorkTicketStatuses(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetWorkTicketStatusDictionariesRequest(), cancellationToken));
    }
    
    /// <summary>Gets all work ticket types</summary>
    [HttpGet("work-ticket-types")]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetWorkTicketTypes(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetWorkTicketTypeDictionariesRequest(), cancellationToken));
    }
    
    
    
    /// <summary>Gets all work task statuses</summary>
    [HttpGet("work-task-statuses")]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetWorkTaskStatuses(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetWorkTaskStatusDictionariesRequest(), cancellationToken));
    }
    
    
    
    /// <summary>Gets all work task statuses</summary>
    [HttpGet("work-item-priority")]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetWorkItemPriorities(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetWorkItemPriorityDictionariesRequest(), cancellationToken));
    }
    
    
    /// <summary>Gets all work group statuses</summary>
    [HttpGet("work-group-statuses")]
    public async Task<ActionResult<IReadOnlyList<DictionaryModel>>> GetWorkGroupStatuses(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetWorkGroupStatusesDictionariesRequest(), cancellationToken));
    }
}
