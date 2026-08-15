using ATMS.Application.Models;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Commands.WorkProjects;
using ATMS.Project.Contracts.Models.WorkProjects;
using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Requests.Users;
using ATMS.Project.Contracts.Requests.WorkProjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Project.API.Controllers.v1;

[Authorize]
[Route("api/v1/project")]
public class ProjectController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns paginated and filtered projects available to the current user.
    /// </summary>
    /// <remarks>
    /// Super administrators can view all projects. Other users can view only projects in which they participate.
    /// Supports optional search, filtering, sorting and pagination via <see cref="GetWorkProjectsRequest"/>.
    /// </remarks>
    /// <param name="request">Query parameters for searching, filtering, sorting and paging.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns a paginated list of projects.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<WorkProjectItemModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<WorkProjectItemModel>>> Index(
        [FromQuery] GetWorkProjectsRequest request, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    /// <summary>
    /// Returns a single project by ID.
    /// </summary>
    /// <remarks>
    /// Retrieves project details, dictionaries and participants when the current user has access to the project.
    /// </remarks>
    /// <param name="id">Project ID (Guid).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the project data.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="404">Project with the specified ID was not found or is unavailable to the current user.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkProjectModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WorkProjectModel>> Get(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetWorkProjectRequest { Id = id }, cancellationToken));
    }

    /// <summary>
    /// Returns users from the internal team who can participate in projects.
    /// </summary>
    /// <remarks>
    /// Client organization users are intentionally excluded. They are loaded from the selected organization instead.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns internal project participant candidates.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet("team-members")]
    [ProducesResponseType(typeof(UserModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserModel[]>> GetTeamMembers(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetProjectTeamMembersRequest(), cancellationToken));
    }

    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <remarks>
    /// Creates a project with an automatically generated code. Only a super administrator can perform this operation.
    /// Internal projects cannot have an organization. Participants may be added without an organization when they belong to the internal team.
    /// </remarks>
    /// <param name="command">Command containing project details and optional participants.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Project successfully created.</response>
    /// <response code="400">Validation error, e.g. invalid dates, dictionaries or participants.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden, user is not a super administrator.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreateWorkProjectCommand command, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(Get), new { id }, id);
    }

    /// <summary>
    /// Updates an existing project.
    /// </summary>
    /// <remarks>
    /// Updates project details and replaces its participant list. Only a super administrator can perform this operation.
    /// </remarks>
    /// <param name="command">Command containing updated project details and participants.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Project successfully updated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden, user is not a super administrator.</response>
    /// <response code="404">Project with the specified ID was not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        [FromBody] UpdateWorkProjectCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Changes the status of an existing project.
    /// </summary>
    /// <remarks>
    /// Updates only the project status. Only a super administrator can perform this operation.
    /// </remarks>
    /// <param name="id">Project ID.</param>
    /// <param name="command">New project status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Project status successfully updated.</response>
    /// <response code="400">Validation error or unsupported project status.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden, user is not a super administrator.</response>
    /// <response code="404">Project with the specified ID was not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPatch("status/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateStatus(
        Guid id, [FromBody] UpdateWorkProjectStatusCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes an existing project.
    /// </summary>
    /// <remarks>
    /// Soft-deletes the project and its participant assignments. Only a super administrator can perform this operation.
    /// </remarks>
    /// <param name="id">Project ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Project successfully deleted.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="403">Resource forbidden, user is not a super administrator.</response>
    /// <response code="404">Project with the specified ID was not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteWorkProjectCommand { Id = id }, cancellationToken);
        return NoContent();
    }
}
