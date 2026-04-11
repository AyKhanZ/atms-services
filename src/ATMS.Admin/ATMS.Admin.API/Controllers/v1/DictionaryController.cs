using ATMS.Admin.Contracts.Models.Dictionaries;
using ATMS.Admin.Contracts.Requests.Dictionaries;
using ATMS.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[Authorize]
[Route("api/v1/dictionary")]
public class DictionaryController(IMediator mediator) : AdminControllerBase
{
    
    /// <summary>Gets all genders</summary>
    [HttpGet("user-types")]
    public async Task<ActionResult<DictionaryModel[]>> GetUserTypes(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetUserTypesDictionariesRequest(), cancellationToken));
    }
    
    /// <summary>Gets all genders</summary>
    [HttpGet("genders")]
    public async Task<ActionResult<DictionaryModel[]>> GetGenders(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetGenderDictionariesRequest(), cancellationToken));
    }

    /// <summary>Gets all marital statuses</summary>
    [HttpGet("marital-statuses")]
    public async Task<ActionResult<DictionaryModel[]>> GetMaritalStatuses(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetMaritalStatusDictionariesRequest(), cancellationToken));
    }

    /// <summary>Gets all user statuses</summary>
    [HttpGet("user-statuses")]
    public async Task<ActionResult<IReadOnlyList<PermissionModel>>> GetUserStatuses(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetUserStatusDictionariesRequest(), cancellationToken));
    }
    
    /// <summary>Gets all permissions</summary>
    [HttpGet("permissions")]
    public async Task<ActionResult<PermissionModel[]>> GetPermissions(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetPermissionDictionariesRequest(), cancellationToken));
    }
}
