using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Users;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UserModel[]>> Index([FromQuery] GetUsersRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(request, cancellationToken);

        return Ok(result);
    }


    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserModel>> Get(Guid id,  CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserRequest { Id = id }, cancellationToken);

        return Ok(result);
    }
}
