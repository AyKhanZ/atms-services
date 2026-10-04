using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Contracts.Models.Profile;
using ATMS.Admin.Contracts.Requests.Profile;
using ATMS.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[Authorize]
[ApiController]
[Route("api/v1/profile")]
public class ProfileController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Gets the authenticated user's personal settings. Super admins cannot use personal settings.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The current user's profile.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="403">Personal settings are unavailable to super admins.</response>
    /// <response code="404">The current user was not found.</response>
    /// <response code="409">The user must complete onboarding before opening settings.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ProfileModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProfileModel>> Get(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetProfileRequest(), cancellationToken));
    }

    /// <summary>
    /// Saves the authenticated user's personal details, language, and optional replacement photo.
    /// </summary>
    /// <param name="command">Personal information and an optional image file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The updated profile.</response>
    /// <response code="400">The submitted information or image is invalid.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="403">Personal settings are unavailable to super admins.</response>
    /// <response code="404">The current user was not found.</response>
    /// <response code="409">The user must complete onboarding first.</response>
    [HttpPut("settings")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ProfileModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProfileModel>> UpdateSettings(
        [FromForm] UpdateSettingsCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Changes the authenticated user's preferred language.
    /// </summary>
    /// <param name="command">The language code to select.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">The language was changed.</response>
    /// <response code="400">The language code is invalid.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="403">Personal settings are unavailable to super admins.</response>
    /// <response code="404">The current user or language was not found.</response>
    /// <response code="409">The user must complete onboarding first.</response>
    [HttpPatch("language")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateLanguage(
        [FromBody] UpdateLanguageCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
