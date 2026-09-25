using ATMS.Application.Models;
using ATMS.Project.Contracts.Models.Dashboard;
using ATMS.Project.Contracts.Requests.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Project.API.Controllers.v1;

[Authorize]
[Route("api/v1/dashboard")]
public sealed class DashboardController(IMediator mediator) : ControllerBase
{
    /// <summary>Returns dashboard metrics, charts, deadlines and recent activity.</summary>
    /// <remarks>
    /// Includes tasks and subtasks from all projects the caller may view, or only projectId when
    /// supplied. Period is 7, 30 or 90 business days, including today in the business time zone.
    /// Recent activity includes the current subject code and title; deleted subjects are marked
    /// so clients can show the entry without linking to the deleted item. Client users receive a null workload.
    /// </remarks>
    /// <response code="200">The complete dashboard.</response>
    /// <response code="400">Period is not 7, 30 or 90.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot view projects.</response>
    /// <response code="404">The selected project was not found among accessible projects.</response>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardModel>> Get(
        [FromQuery] GetDashboardRequest request,
        CancellationToken cancellationToken) =>
        Ok(await mediator.Send(request, cancellationToken));
}
