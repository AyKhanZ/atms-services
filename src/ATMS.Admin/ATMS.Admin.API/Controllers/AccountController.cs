using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Models;
using ATMS.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers;

[Route("api/account")]
public class AccountController(IMediator mediator) : AdminControllerBase
{
    
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
    /// <response code="500">Unhandled server error.</response>
    [Authorize]
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel),StatusCodes.Status403Forbidden)]
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


    //[HttpGet("confirm-email")]
    //public async Task<IActionResult> ConfirmEmail(string token)
    // {
    //     var isConfirmed = await mediator.Send(token);
    //     return isConfirmed ? Redirect(urlSettings.EmailConfirmedPage) : BadRequest("Invalid Token");
    // }
    
    //[HttpPost("resend/email-confirmation")]
    //public async Task<ActionResult> ResendConfirmationLetter([FromHeader] string email)
    // {
    //     var result = await mediator.Send(email);
    //     return Ok(result);
    // }

    //[HttpPost("send-email")]
    //public async Task<ActionResult> SendLoginDetailsEmail([FromBody] SendLoginDataCommand command)
    // {
    //     var result = await mediator.Send(command);
    //     return Ok(result);
    // }

    



    // [HttpPut("change-password")]
    // public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken cancellationToken)
    // {
    //     var result = await mediator.Send(command, cancellationToken);
    //     return Ok(result);
    // }

    // [HttpPost("forgot-password")]
    // public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
    // {
    //     var result = await mediator.Send(command, cancellationToken);
    //     return Ok(result);
    // }

    // [HttpPost("reset-password")]
    // public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
    // {
    //     var result = await mediator.Send(command, cancellationToken);
    //     return Ok(result);
    // }
}
