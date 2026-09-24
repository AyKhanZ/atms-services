using ATMS.Application.Models;
using ATMS.Project.API.Results;
using ATMS.Project.Contracts.Commands.Attachments;
using ATMS.Project.Contracts.Models.Attachments;
using ATMS.Project.Contracts.Requests.Attachments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Project.API.Controllers.v1;

[Authorize]
[Route("api/v1/project/{projectId:guid}")]
public class AttachmentController(IMediator mediator) : ControllerBase
{
    // 25 MB of file plus room for the multipart envelope.
    private const long MaxUploadRequestBytes = 26 * 1024 * 1024;

    /// <summary>
    /// Uploads one file to a task or subtask.
    /// </summary>
    /// <remarks>
    /// Send one file per request in the multipart field "file". Files are accepted up to 25 MB, only
    /// PDF, Office, OpenDocument, JPG, PNG, WEBP, GIF, TXT, CSV and ZIP, and only when the content
    /// matches the extension. A task or subtask holds up to 100 files.
    /// </remarks>
    /// <response code="201">The file was uploaded.</response>
    /// <response code="400">The file is missing, empty, too large, of an unsupported type, or the task is full.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot edit tasks in this project.</response>
    /// <response code="404">The task or the file was not found.</response>
    /// <response code="413">The request is larger than the upload limit.</response>
    [HttpPost("work-tasks/{workTaskId:guid}/attachments")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadRequestBytes)]
    [ProducesResponseType(typeof(AttachmentModel), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AttachmentModel>> Upload(Guid projectId, Guid workTaskId, [FromForm] UploadAttachmentCommand command, CancellationToken cancellationToken)
    {
        command.ProjectId = projectId;
        command.WorkTaskId = workTaskId;

        var attachment = await mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetContent), new { projectId, attachmentId = attachment.Id }, attachment);
    }


    /// <summary>
    /// Returns the files of one task, of a task's subtasks, or of a whole ticket.
    /// </summary>
    /// <remarks>
    /// Pass exactly one of workTaskId, parentWorkTaskId or workTicketId. Newest files come first.
    /// Every file carries its task and, for a subtask, the parent task, so the client can draw the
    /// hierarchy. At most 1000 files are returned; hasMore tells that the list was cut.
    /// </remarks>
    /// <response code="200">Returns the files.</response>
    /// <response code="400">None or more than one scope was given.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot view this project.</response>
    [HttpGet("attachments")]
    [ProducesResponseType(typeof(AttachmentListModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AttachmentListModel>> GetMany(Guid projectId, [FromQuery] GetAttachmentsRequest request, CancellationToken cancellationToken)
    {
        request.ProjectId = projectId;
        return Ok(await mediator.Send(request, cancellationToken));
    }


    /// <summary>
    /// Returns the project's groups, milestones and tickets that hold files, with file counts.
    /// </summary>
    /// <remarks>
    /// Branches without files are left out. The files themselves are read per ticket with
    /// GET attachments?workTicketId=.
    /// </remarks>
    /// <response code="200">Returns the tree.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot view this project.</response>
    [HttpGet("attachments/tree")]
    [ProducesResponseType(typeof(AttachmentTreeModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AttachmentTreeModel>> GetTree(Guid projectId, CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetAttachmentsTreeRequest { ProjectId = projectId }, cancellationToken));
    }


    /// <summary>
    /// Returns the file itself.
    /// </summary>
    /// <remarks>
    /// By default the file is sent as a download under its original name. inline=true shows images, text
    /// and PDF in the browser instead; other types are always downloaded.
    /// </remarks>
    /// <response code="200">Returns the file.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot view this project.</response>
    /// <response code="404">The file was not found or is no longer on the server.</response>
    [HttpGet("attachments/{attachmentId:guid}/content")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetContent(Guid projectId, Guid attachmentId, [FromQuery] bool inline, CancellationToken cancellationToken)
    {
        var content = await mediator.Send(new GetAttachmentContentRequest
        {
            ProjectId = projectId,
            AttachmentId = attachmentId
        }, cancellationToken);

        return new AttachmentContentResult(content, attachmentId, inline);
    }


    /// <summary>
    /// Renames a file.
    /// </summary>
    /// <remarks>
    /// fileName is the name without the extension; the extension set at upload is kept.
    /// </remarks>
    /// <response code="204">The file was renamed.</response>
    /// <response code="400">The name is empty, too long or has characters a file name cannot have.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot edit tasks in this project.</response>
    /// <response code="404">The file was not found; it may have been deleted.</response>
    [HttpPatch("attachments/{attachmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Rename(Guid projectId, Guid attachmentId, [FromBody] RenameAttachmentCommand command, CancellationToken cancellationToken)
    {
        command.ProjectId = projectId;
        command.AttachmentId = attachmentId;

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }


    /// <summary>
    /// Deletes a file.
    /// </summary>
    /// <remarks>
    /// The deletion is soft: the file disappears from every list but stays on the server.
    /// </remarks>
    /// <response code="204">The file was deleted.</response>
    /// <response code="400">The file was not found in this project.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user cannot edit tasks in this project.</response>
    /// <response code="404">The file was not found; it may have been deleted.</response>
    [HttpDelete("attachments/{attachmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid projectId, Guid attachmentId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteAttachmentCommand
        {
            ProjectId = projectId,
            AttachmentId = attachmentId
        }, cancellationToken);

        return NoContent();
    }
}
