using ATMS.Admin.Contracts.Commands.Onboarding;
using ATMS.Admin.Contracts.Models.Onboarding;
using ATMS.Admin.Contracts.Requests.Onboarding;
using ATMS.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[Authorize(Policy = "OnboardingAccess")]
[ApiController]
[Route("api/v1/onboarding")]
public sealed class OnboardingController(IMediator mediator) : ControllerBase
{

    /// <summary>
    /// Gets the current user's onboarding progress.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Current onboarding state and saved data.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(OnboardingModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OnboardingModel>> Get(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetOnboardingRequest(), cancellationToken));
    }


    /// <summary>
    /// Saves the current user's personal onboarding information.
    /// </summary>
    /// <param name="command">Personal information and the latest onboarding version.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Updated onboarding state.</response>
    /// <response code="400">The submitted information is invalid.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="409">The onboarding state was changed by another request.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPut("personal-info")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(OnboardingModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OnboardingModel>> SavePersonalInfo(
        [FromForm] SavePersonalInfoCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command, cancellationToken));
    }


    /// <summary>
    /// Saves a new password for activation when onboarding is completed.
    /// </summary>
    /// <param name="command">New password and the latest onboarding version.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Updated onboarding state.</response>
    /// <response code="400">The password is invalid.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="409">The onboarding state was changed by another request.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPut("security")]
    [ProducesResponseType(typeof(OnboardingModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OnboardingModel>> SaveSecurity(
        [FromBody] SaveSecurityCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command, cancellationToken));
    }


    /// <summary>
    /// Saves colleagues to invite after onboarding is completed.
    /// </summary>
    /// <param name="command">Invited colleagues and the latest onboarding version.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Updated onboarding state.</response>
    /// <response code="400">The invitation list is invalid.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="409">The onboarding state was changed by another request.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPut("invitations")]
    [ProducesResponseType(typeof(OnboardingModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OnboardingModel>> SaveInvitations(
        [FromBody] SaveInvitationsCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command, cancellationToken));
    }


    /// <summary>
    /// Skips the optional colleague invitation step.
    /// </summary>
    /// <param name="command">Latest onboarding version.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Updated onboarding state.</response>
    /// <response code="400">The invitation step is unavailable.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="409">The onboarding state was changed by another request.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPost("invitations/skip")]
    [ProducesResponseType(typeof(OnboardingModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OnboardingModel>> SkipInvitations(
        [FromBody] SkipInvitationsCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command, cancellationToken));
    }


    /// <summary>
    /// Completes onboarding and activates the saved profile and password.
    /// </summary>
    /// <param name="command">Latest onboarding version.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">A new access token and invitation queue result.</response>
    /// <response code="400">A required onboarding step is incomplete.</response>
    /// <response code="401">The request is not authenticated.</response>
    /// <response code="409">The onboarding state was changed by another request.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPost("complete")]
    [ProducesResponseType(typeof(OnboardingCompletionModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OnboardingCompletionModel>> Complete(
        [FromBody] CompleteOnboardingCommand command, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(command, cancellationToken));
    }
}
