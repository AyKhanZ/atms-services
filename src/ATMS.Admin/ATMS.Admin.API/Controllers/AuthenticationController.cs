using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController(IMediator mediator) : ControllerBase
{
}