using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[Authorize]
[Route("api/v1/profile/{id:guid}")]
public class ProfileController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Updates the settings of the user.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <param name="command">Command containing settings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Settings successfully updated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized.</response>
    /// <response code="404">User not found.</response>
    /// <response code="500">Unhandled server error.</response>
    [HttpPut("settings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSettings(Guid id, [FromBody] UpdateSettingsCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await mediator.Send(command, cancellationToken);
        
        return NoContent();
    }


    /// <summary>
    /// Changes the photo of the user.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <param name="command">Command containing file name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Photo successfully changed.</response>
    /// <response code="400">Validation error, e.g., password format invalid or missing fields.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="404">User with specified ID not found.</response>
    /// <response code="500">Unhandled server error.</response>
    [HttpPatch("photo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePhoto(Guid id, [FromBody] UpdatePhotoCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await mediator.Send(command, cancellationToken);
        
        return NoContent();
    }


    /// <summary>
    /// Changes the language of the user interface.
    /// </summary>
    /// <param name="id">User ID.</param>
    /// <param name="command">Command containing language.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Language successfully changed.</response>
    /// <response code="400">Validation error, e.g., password format invalid or missing fields.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="404">User with specified ID not found.</response>
    /// <response code="500">Unhandled server error.</response>
    [HttpPatch("language")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateLanguage(Guid id, [FromBody] UpdateLanguageCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await mediator.Send(command, cancellationToken);
        
        return NoContent();
    }
}