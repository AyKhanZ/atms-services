using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers;

[ApiController]
[Route("api/me")]
public class MeController(IMediator mediator) : ControllerBase
{
}
