using ATMS.Application.Models;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Commands.WorkTasks;
using ATMS.Project.Contracts.Models.WorkTasks;
using ATMS.Project.Contracts.Requests.WorkTasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Project.API.Controllers.v1;

[Authorize]
[Route("api/v1/project/{projectId:guid}/work-tasks")]
public class WorkTaskController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns a cursor-paginated page of tasks in the selected project.
    /// </summary>
    /// <remarks>
    /// Filter by workTicketId with rootTasksOnly=true for a ticket's top-level tasks, or by
    /// parentWorkTaskId for one task's subtasks. Progress counts are included for every returned task.
    /// </remarks>
    /// <response code="200">Returns tasks visible in the selected project.</response>
    /// <response code="400">The hierarchy filters or pagination settings are invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot view this project.</response>
    /// <response code="404">The project was not found.</response>
    [HttpGet]
    [ProducesResponseType(typeof(KeysetPagedResult<WorkTaskModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<KeysetPagedResult<WorkTaskModel>>> GetMany(Guid projectId, [FromQuery] GetWorkTasksRequest request, CancellationToken cancellationToken)
    {
        request.ProjectId = projectId;
        return Ok(await mediator.Send(request, cancellationToken));
    }
    

    /// <summary>
    /// Returns one task/subtask from the selected project.
    /// </summary>
    /// <remarks>
    /// The response includes ticket location, parent task details, status, priority, assignee and subtask progress.
    /// </remarks>
    /// <response code="200">Returns the requested task.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot view this project.</response>
    /// <response code="404">The project or task was not found.</response>
    [HttpGet("{workTaskId:guid}")]
    [ProducesResponseType(typeof(WorkTaskModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WorkTaskModel>> Get(Guid projectId, Guid workTaskId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetWorkTaskRequest
        {
            ProjectId = projectId,
            WorkTaskId = workTaskId
        }, cancellationToken));
    }
    

    /// <summary>
    /// Creates a task/subtask.
    /// </summary>
    /// <remarks>
    /// New items always start in New status. parentWorkTaskId creates a subtask; workTicketId remains required
    /// and must match the parent task's ticket. A subtask cannot contain another subtask. Assignees must be staff participants.
    /// </remarks>
    /// <response code="201">The task was created.</response>
    /// <response code="400">The task data, hierarchy, dictionary value or assignee is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot create tasks in this project.</response>
    /// <response code="404">The project, ticket or parent task was not found.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateWorkTaskCommand command, CancellationToken cancellationToken)
    {
        command.ProjectId = projectId;
        var id = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { projectId, workTaskId = id }, id);
    }
    

    /// <summary>
    /// Updates a task or subtask without moving it in the hierarchy.
    /// </summary>
    /// <remarks>
    /// The route IDs take precedence over body values. Parent task and ticket cannot be changed.
    /// </remarks>
    /// <response code="204">The task was updated.</response>
    /// <response code="400">The task data, status, priority or assignee is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot edit tasks in this project.</response>
    /// <response code="404">The project or task was not found.</response>
    [HttpPut("{workTaskId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid projectId, Guid workTaskId, [FromBody] UpdateWorkTaskCommand command, CancellationToken cancellationToken)
    {
        command.ProjectId = projectId;
        command.WorkTaskId = workTaskId;
        
        await mediator.Send(command, cancellationToken);
        
        return NoContent();
    }

    
    /// <summary>
    /// Deletes a task/subtask.
    /// </summary>
    /// <remarks>
    /// A task with subtasks cannot be deleted. Delete its subtasks first.
    /// </remarks>
    /// <response code="204">The task was deleted.</response>
    /// <response code="400">The task still has subtasks and cannot be deleted.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot delete tasks in this project.</response>
    /// <response code="404">The project or task was not found.</response>
    [HttpDelete("{workTaskId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid projectId, Guid workTaskId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteWorkTaskCommand
        {
            ProjectId = projectId,
            WorkTaskId = workTaskId
        }, cancellationToken);
        return NoContent();
    }
}
