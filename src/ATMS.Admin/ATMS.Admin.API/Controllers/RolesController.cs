using ATMS.Admin.Contracts.Commands;
using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Roles;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers;

[Route("api/roles")]
[ApiController]
public class RolesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<RoleModel[]>> Index([FromQuery] GetRolesRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet(":id")]
    public async Task<ActionResult<RoleModel>> Get([FromQuery] GetRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<RoleModel>> Create([FromBody] CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult<RoleModel>> Update([FromBody] UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<ActionResult<Guid>> Delete([FromQuery] DeleteRoleCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
