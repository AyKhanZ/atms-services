using ATMS.Application.Models;
using ATMS.Project.Contracts.Commands.WorkGroups;
using ATMS.Project.Contracts.Models.WorkGroups;
using ATMS.Project.Contracts.Requests.WorkGroups;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Project.API.Controllers.v1;

[Authorize]
[Route("api/v1/project/{projectId:guid}/work-groups")]
public class WorkGroupController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns the project's root groups with their milestones.
    /// </summary>
    /// <remarks>
    /// The result is not paginated because a project is expected to have a small number of groups and milestones.
    /// </remarks>
    /// <param name="projectId">Project ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Returns the nested work groups.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="404">Project was not found.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(WorkGroupModel[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WorkGroupModel[]>> GetGroups(Guid projectId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetWorkGroupsRequest { ProjectId = projectId }, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Creates a group or milestone in the project.
    /// </summary>
    /// <remarks>
    /// Omit <c>parentWorkGroupId</c> to create a root group. Supply the ID of a root group from the same project
    /// to create a milestone.
    /// </remarks>
    /// <param name="projectId">Project ID.</param>
    /// <param name="command">Name and optional parent group.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Group or milestone successfully created.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="404">Project or parent group was not found.</response>
    /// <response code="409">A sibling with the same name already exists.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        Guid projectId,
        [FromBody] CreateWorkGroupCommand command,
        CancellationToken cancellationToken)
    {
        command.ProjectId = projectId;
        var id = await mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetGroups), new { projectId }, id);
    }

    /// <summary>
    /// Renames a group or milestone.
    /// </summary>
    /// <remarks>
    /// Moving an item to another group is not supported.
    /// </remarks>
    /// <param name="projectId">Project ID.</param>
    /// <param name="workGroupId">Group or milestone ID.</param>
    /// <param name="command">Updated name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Group or milestone successfully updated.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="404">Group or milestone was not found in this project.</response>
    /// <response code="409">A sibling with the same name already exists.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPut("{workGroupId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        Guid projectId,
        Guid workGroupId,
        [FromBody] UpdateWorkGroupCommand command,
        CancellationToken cancellationToken)
    {
        command.ProjectId = projectId;
        command.WorkGroupId = workGroupId;
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes an empty group or milestone.
    /// </summary>
    /// <remarks>
    /// Deletion is soft. A group containing milestones or tickets and a milestone containing tickets cannot be deleted.
    /// </remarks>
    /// <param name="projectId">Project ID.</param>
    /// <param name="workGroupId">Group or milestone ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Group or milestone successfully deleted.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized, user is not authenticated.</response>
    /// <response code="404">Group or milestone was not found in this project.</response>
    /// <response code="409">The item is not empty and cannot be deleted.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpDelete("{workGroupId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
        Guid projectId,
        Guid workGroupId,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteWorkGroupCommand
        {
            ProjectId = projectId,
            WorkGroupId = workGroupId
        }, cancellationToken);

        return NoContent();
    }
}
