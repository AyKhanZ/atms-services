using ATMS.Admin.Contracts.Commands.Migration;
using ATMS.Admin.Contracts.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers;

[ApiController]
[Route("api/migrations")]
public class MigrationController(IMediator mediator) : ControllerBase
{
    [HttpPost("up")]
    public async Task<ActionResult<MigrationModel>> Up([FromQuery] ApplyMigrationsCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    [HttpPost("down")]
    public async Task<ActionResult<MigrationModel>> Down([FromQuery] DownMigrationCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}
