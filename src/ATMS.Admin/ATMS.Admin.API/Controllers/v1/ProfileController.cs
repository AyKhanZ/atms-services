using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Admin.API.Controllers.v1;

[Authorize]
[Microsoft.AspNetCore.Components.Route("api/v1/profile")]
public class ProfileController(IMediator mediator) : AdminControllerBase
{
    /*
     * 
     * Language patch
     * 
     * Change settings put
     *
     * ProfilePhoto patch
     * 
     */
    //
    // [HttpPatch("language")]
    // public async Task<IActionResult> UpdateLanguage([FromBody] UpdateLanguageCommand command)
    // {
    //     await mediator.Send(command);
    //     return NoContent();
    // }
}
