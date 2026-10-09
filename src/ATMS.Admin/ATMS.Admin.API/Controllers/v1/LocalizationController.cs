using ATMS.Admin.Contracts.Models.Localization;
using ATMS.Admin.Contracts.Requests.Localization;
using ATMS.Application.Models;
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
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(DefaultLanguageModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DefaultLanguageModel>> Get(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetDefaultLanguageRequest(), cancellationToken));
    }
}
