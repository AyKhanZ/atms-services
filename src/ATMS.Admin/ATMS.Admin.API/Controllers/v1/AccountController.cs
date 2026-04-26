using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Models.Users;
using ATMS.Application.Models;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[Route("api/v1/account")]
public class AccountController(IMediator mediator, IConfiguration configuration) : ControllerBase
{

    private readonly RedirectUrlOptions _redirectUrlOptions =
        configuration.GetSection(nameof(RedirectUrlOptions)).Get<RedirectUrlOptions>() 
        ?? throw new ConfigurationException(ConfigurationErrorType.RedirectUrlSectionNotFound,
            string.Format(ExceptionMessages.ConfigSectionNotFound, nameof(RedirectUrlOptions)));

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <remarks>
    /// Creates a user account and returns the created user information.
    /// Email must be unique. Password must meet security requirements.
    /// </remarks>
    /// <param name="command">Register request containing user details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">User successfully created.</response>
    /// <response code="400">Validation error, e.g., missing fields or invalid data.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="404">Role with specified ID not found.</response>
    /// <response code="500">Unhandled server error.</response>
    [Authorize]
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserModel>> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var user = await mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            actionName: nameof(UsersController.Get),
            controllerName: "Users",
            routeValues: new { id = user.Id },
            value: user);
    }




    /// <summary>
    /// Confirms user email using a confirmation token.
    /// </summary>
    /// <remarks>
    /// This endpoint is called when the user clicks the confirmation link
    /// sent to their email address.
    ///
    /// The endpoint validates the email confirmation token and, if valid, marks the user's email as confirmed.
    ///
    /// After processing, the user is redirected to the appropriate page:
    ///
    /// - Success → Email confirmed page
    /// - Failure → Email confirmation failed page
    ///
    /// Example request:
    ///
    ///     GET /api/v1/email/confirm?token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
    ///
    /// </remarks>
    /// <param name="token">Email confirmation token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="302">Redirects user to confirmation result page.</response>
    /// <response code="400">Invalid or malformed request.</response>
    /// <response code="409">Email already confirmed.</response>
    /// <response code="500">Unexpected server error.</response>
    [AllowAnonymous]
    [HttpGet("confirm")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmEmail(
        [FromQuery] string token,
        CancellationToken cancellationToken)
    {
        var isConfirmed = await mediator.Send(new ConfirmEmailCommand { Token = token }, cancellationToken);

        return isConfirmed
            ? Redirect(_redirectUrlOptions.EmailConfirmedPage)
            : Redirect(_redirectUrlOptions.EmailConfirmFailedPage);
    }


    /// <summary>
    /// Resends the email confirmation link.
    /// </summary>
    /// <remarks>
    /// Sends a new email confirmation link to the specified email address.
    ///
    /// This endpoint is typically used when a user did not receive the original
    /// confirmation email or when the confirmation token has expired.
    ///
    /// If an account with the specified email exists and the email is not yet confirmed,
    /// a new confirmation email will be sent.
    ///
    /// For security reasons, the response is identical regardless of whether the email
    /// exists in the system, preventing account enumeration attacks.
    ///
    /// Example request:
    ///
    ///     POST /api/v1/account/email-confirmation/resend
    ///     {
    ///         "email": "user@example.com"
    ///     }
    ///
    /// </remarks>
    /// <param name="command">Command containing the user's email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="202">Request accepted. If the account exists and is not yet confirmed, a confirmation email will be sent.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">User with specified ID not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPost("email-confirmation/resend")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResendConfirmationLetter(
        [FromBody] ResendEmailConfirmationCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);

        return Accepted();
    }




    /// <summary>
    /// Changes the password of the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// User must provide their current password (OldPassword) and a new password (NewPassword).
    /// New password must meet security requirements
    /// (at least 1 uppercase letter, 1 number, 1 special char from !@#$%^&*()-_=+, no spaces, length 6-40).
    /// </remarks>
    /// <param name="command">Command containing Email, OldPassword, and NewPassword.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Password successfully changed.</response>
    /// <response code="400">Validation error, e.g., password format invalid or missing fields.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="404">User with specified ID not found.</response>
    /// <response code="500">Unhandled server error.</response>
    [Authorize]
    [HttpPut("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }


    /// <summary>
    /// Initiates the password reset process.
    /// </summary>
    /// <remarks>
    /// This endpoint starts the password reset flow for a user who forgot their password.
    ///
    /// If the email exists in the system, a password reset link containing a reset token
    /// will be sent to the user's email address.
    ///
    /// For security reasons, the response is the same whether the email exists or not,
    /// preventing user enumeration attacks.
    /// 
    /// Example request:
    ///
    ///     POST /api/v1/account/forgot-password
    ///     {
    ///         "email": "user@example.com"
    ///     }
    ///
    /// </remarks>
    /// <param name="command">Command containing the reset token, new password, and password confirmation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="202">Request accepted. If the account exists, a password reset email will be sent.</response>
    /// <response code="400">Invalid email format or validation error.</response>
    /// <response code="404">User with specified ID not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);

        return Accepted();
    }


    /// <summary>
    /// Resets the user's password using a valid password reset token.
    /// </summary>
    /// <remarks>
    /// Completes the password reset process after the user follows the reset link
    /// received via email.
    ///
    /// The request must contain:
    /// - a valid password reset token
    /// - a new password
    /// - a confirmation password
    ///
    /// The token must:
    /// - exist in the system
    /// - not be expired
    ///
    /// If the token is valid, the user's password will be updated and all
    /// password reset tokens associated with the user will be invalidated.
    ///
    /// Example request:
    ///
    ///     POST /api/v1/account/reset-password
    ///     {
    ///         "token": "reset-token-value",
    ///         "password": "NewPassword123!",
    ///         "confirmPassword": "NewPassword123!"
    ///     }
    ///
    /// </remarks>
    /// <param name="command">Command containing email address associated with the user account.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Password successfully reset.</response>
    /// <response code="400">Invalid email format or validation error.</response>
    /// <response code="404">User with specified ID not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
}
