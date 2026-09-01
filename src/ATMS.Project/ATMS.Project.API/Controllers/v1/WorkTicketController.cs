using ATMS.Application.Models;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Commands.WorkTickets;
using ATMS.Project.Contracts.Models.WorkTickets;
using ATMS.Project.Contracts.Requests.WorkTickets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Project.API.Controllers.v1;

[Authorize]
[Route("api/v1/project/{projectId:guid}/work-tickets")]
public class WorkTicketController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns a cursor-paginated page of tickets available in the selected project's plan.
    /// </summary>
    /// <remarks>
    /// The result contains the ticket's milestone, parent group, type, priority, status and optional assignee.
    /// Only users with permission to view the selected project can access the list.
    /// </remarks>
    /// <param name="projectId">Project ID.</param>
    /// <param name="request">Cursor pagination settings and optional milestone filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the tickets in the selected project.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden, user cannot view tickets in this project.</response>
    /// <response code="404">Project with the specified ID was not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(KeysetPagedResult<WorkTicketModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<KeysetPagedResult<WorkTicketModel>>> GetMany(
        Guid projectId,
        [FromQuery] GetWorkTicketsRequest request,
        CancellationToken cancellationToken)
    {
        request.ProjectId = projectId;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    /// <summary>
    /// Returns a single ticket from the selected project.
    /// </summary>
    /// <remarks>
    /// The ticket is returned only when it belongs to the project from the route and the current user has permission
    /// to view the selected project.
    /// </remarks>
    /// <param name="projectId">Project ID.</param>
    /// <param name="workTicketId">Ticket ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the requested ticket.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden, user cannot view tickets in this project.</response>
    /// <response code="404">Project or ticket with the specified ID was not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet("{workTicketId:guid}")]
    [ProducesResponseType(typeof(WorkTicketModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WorkTicketModel>> Get(Guid projectId, Guid workTicketId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetWorkTicketRequest
        {
            ProjectId = projectId,
            WorkTicketId = workTicketId
        }, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Creates a new ticket in a project milestone.
    /// </summary>
    /// <remarks>
    /// The milestone and optional assignee must belong to the project from the route. The backend derives the parent
    /// group from the milestone and assigns the initial New status. The client cannot select the initial status.
    /// </remarks>
    /// <param name="projectId">Project ID.</param>
    /// <param name="command">Command containing the ticket name, milestone, type, priority and optional details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Ticket successfully created.</response>
    /// <response code="400">Validation error, e.g. missing name or invalid milestone, type, priority or assignee.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden, user cannot edit tickets in this project.</response>
    /// <response code="404">Project or milestone with the specified ID was not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateWorkTicketCommand command, CancellationToken cancellationToken)
    {
        command.ProjectId = projectId;
        var id = await mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(Get), new { projectId, workTicketId = id }, id);
    }

    /// <summary>
    /// Updates the editable details of an existing ticket.
    /// </summary>
    /// <remarks>
    /// Updates the ticket only when it belongs to the project from the route. The selected milestone and optional
    /// assignee must also belong to that project. The selected status must be one of the supported ticket statuses.
    /// </remarks>
    /// <param name="projectId">Project ID.</param>
    /// <param name="workTicketId">Ticket ID.</param>
    /// <param name="command">Command containing the updated editable ticket details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Ticket successfully updated.</response>
    /// <response code="400">Validation error, e.g. missing name or invalid milestone, type, priority or assignee.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden, user cannot edit tickets in this project.</response>
    /// <response code="404">Project, ticket or milestone with the specified ID was not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPut("{workTicketId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid projectId, Guid workTicketId, [FromBody] UpdateWorkTicketCommand command, CancellationToken cancellationToken)
    {
        command.ProjectId = projectId;
        command.WorkTicketId = workTicketId;
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a ticket from the selected project.
    /// </summary>
    /// <remarks>
    /// The ticket is soft-deleted only when it belongs to the project from the route and the current user has the
    /// ticket delete permission for that project.
    /// </remarks>
    /// <param name="projectId">Project ID.</param>
    /// <param name="workTicketId">Ticket ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Ticket successfully deleted.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden, user cannot delete tickets in this project.</response>
    /// <response code="404">Project or ticket with the specified ID was not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpDelete("{workTicketId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid projectId, Guid workTicketId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteWorkTicketCommand
        {
            ProjectId = projectId,
            WorkTicketId = workTicketId
        }, cancellationToken);

        return NoContent();
    }
}
