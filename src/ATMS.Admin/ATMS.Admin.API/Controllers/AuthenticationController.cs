using ATMS.Admin.Contracts.Commands.Authentication;
using ATMS.Admin.Contracts.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AccessInfoModel>> LoginAsync([FromBody] LoginCommand command,  CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }


    [HttpPost("refresh")]
    public async Task<ActionResult<AccessInfoModel>> RefreshTokenAsync([FromBody] RefreshTokenCommand command,  CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}