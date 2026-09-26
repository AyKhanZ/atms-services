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
    /// supplied. Period is today, 7d, 30d, thisMonth, 6m, 12m or custom with from and to (yyyy-MM-dd);
    /// every period ends today in the business time zone, a custom one no later than today and no
    /// longer than a year. The main chart is drawn by hour for a single day, by day up to 92 days and
    /// by month beyond; created and done are compared with the period of the same length before.
    /// Recent activity includes the current subject code and title; deleted subjects are marked
    /// so clients can show the entry without linking to the deleted item. Client users receive a null workload.
    /// </remarks>
    /// <response code="200">The complete dashboard.</response>
    /// <response code="400">Unknown period, malformed date, or a custom range that is incomplete, reversed, in the future or longer than a year.</response>
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
