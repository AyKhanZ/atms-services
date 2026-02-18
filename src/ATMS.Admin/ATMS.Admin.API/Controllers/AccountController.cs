using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
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
    //{
    //    var isConfirmed = await mediator.Send(token);
    //    return isConfirmed ? Redirect(urlSettings.EmailConfirmedPage) : BadRequest("Invalid Token");
    //}


    //[HttpPost("send-email")]
    //public async Task<ActionResult> SendLoginDetailsEmail([FromBody] SendLoginDataCommand command)
    //{
    //    var result = await mediator.Send(command);

    //    return Ok(result);
    //}

    //[HttpPost("resend/email-confiramtion")]
    //public async Task<ActionResult> ResendConfirmationLetter([FromHeader] string email)
    //{
    //    var result = await mediator.Send(email);

    //    return Ok(result);
    //}



    [HttpPut("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }


    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }


    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}
