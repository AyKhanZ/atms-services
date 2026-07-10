using ATMS.Admin.Contracts.Requests.Health;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("health")]
public class HealthController(IMediator mediator) : ControllerBase
{
    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "ATMS Admin API",
            checkedAt = DateTimeOffset.UtcNow
        });
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        var isReady = await mediator.Send(new CheckReadinessRequest(), cancellationToken);
        var response = new
        {
            status = isReady ? "Healthy" : "Unhealthy",
            service = "ATMS Admin API",
            checkedAt = DateTimeOffset.UtcNow
        };

        return isReady
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }
}
