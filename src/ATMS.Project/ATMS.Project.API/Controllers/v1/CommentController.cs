using ATMS.Application.Models;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Commands.Comments;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Contracts.Requests.Comments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Project.API.Controllers.v1;

[Authorize]
[Route("api/v1/project/{projectId:guid}/comments")]
public sealed class CommentController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns one page of a task's comments, newest first.
    /// </summary>
    /// <remarks>
    /// Up to 20 comments a page, 50 at most. An unknown task returns an empty page; comments are always newest first.
    /// A deleted comment keeps its place as a placeholder: `isDeleted`, who deleted it and when, no text.
    /// </remarks>
    /// <response code="200">Returns the task comments.</response>
    /// <response code="400">The paging values are invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot view this project.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(KeysetPagedResult<CommentModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<KeysetPagedResult<CommentModel>>> GetMany(
        Guid projectId, [FromQuery] GetCommentsRequest request, CancellationToken cancellationToken)
    {
        request.ProjectId = projectId;
        return Ok(await mediator.Send(request, cancellationToken));
    }

    /// <summary>
    /// Returns one comment, or its placeholder when it was deleted.
    /// </summary>
    /// <remarks>
    /// Read after a pushed change to put that one comment on screen instead of the whole page.
    /// When workTaskId is supplied, the comment must belong to that task.
    /// </remarks>
    /// <response code="200">Returns the comment.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot view this project.</response>
    /// <response code="404">The comment or its task was not found.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CommentModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommentModel>> Get(
        Guid projectId, Guid id, [FromQuery] Guid? workTaskId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetCommentRequest
        {
            ProjectId = projectId,
            CommentId = id,
            WorkTaskId = workTaskId
        }, cancellationToken));
    }

    /// <summary>
    /// Adds a comment to a task or subtask.
    /// </summary>
    /// <remarks>
    /// Text is limited to 2000 characters.
    /// </remarks>
    /// <response code="201">Returns the new comment.</response>
    /// <response code="400">The text or task is invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot comment in this project.</response>
    /// <response code="404">The task was not found.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(CommentModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommentModel>> Create(Guid projectId, [FromBody] CreateCommentCommand command, CancellationToken cancellationToken)
    {
        command.ProjectId = projectId;
        var model = await mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, model);
    }

    /// <summary>
    /// Changes the text of the user's own comment.
    /// </summary>
    /// <response code="200">Returns the updated comment.</response>
    /// <response code="400">The text is invalid or this is another person's comment.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot comment in this project.</response>
    /// <response code="404">The comment was not found.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CommentModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CommentModel>> Update(Guid projectId, Guid id, [FromBody] UpdateCommentCommand command, CancellationToken cancellationToken)
    {
        command.ProjectId = projectId;
        command.CommentId = id;
        return Ok(await mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Soft-deletes a comment. Authors may delete their own; project managers and administrators may delete others.
    /// </summary>
    /// <response code="204">The comment was deleted.</response>
    /// <response code="400">The comment id is missing or the comment was not found.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot comment in this project, or the comment is another author's and the user has no Comment delete.</response>
    /// <response code="404">The comment was deleted by someone else while the request was on its way.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid projectId, Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteCommentCommand
        {
            ProjectId = projectId,
            CommentId = id
        }, cancellationToken);
        return NoContent();
    }
}
