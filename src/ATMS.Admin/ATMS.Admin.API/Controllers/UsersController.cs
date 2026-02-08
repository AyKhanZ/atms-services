using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IMediator mediator) : ControllerBase
{

}
