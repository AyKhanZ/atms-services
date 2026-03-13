using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers;

[Route("api/me")]
public class MeController(IMediator mediator) : AdminControllerBase
{
}
