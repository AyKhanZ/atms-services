using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[Route("v1/api/me")]
public class MeController(IMediator mediator) : AdminControllerBase
{
}
