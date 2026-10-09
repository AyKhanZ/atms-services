using ATMS.Admin.Contracts.Models.Localization;
using ATMS.Admin.Contracts.Requests.Localization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[AllowAnonymous]
[ApiController]
[Route("api/v1/localization")]
public sealed class LocalizationController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Gets the language used when the caller has not chosen one.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The configured default language.</response>
    [HttpGet]
    [ProducesResponseType(typeof(DefaultLanguageModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<DefaultLanguageModel>> Get(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetDefaultLanguageRequest(), cancellationToken));
    }
}
