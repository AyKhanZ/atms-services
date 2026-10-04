using ATMS.Application.Models;
using ATMS.Data.Criteria;
using ATMS.Project.Contracts.Commands.Notifications;
using ATMS.Project.Contracts.Models.Notifications;
using ATMS.Project.Contracts.Requests.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ATMS.Project.API.Controllers.v1;

[Authorize]
[Route("api/v1/notifications")]
public sealed class NotificationController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Returns one page of the current user's notifications, newest first.
    /// </summary>
    /// <remarks>
    /// Only the caller's own notifications. Up to 20 a page, 50 at most; pass the nextCursor of the
    /// previous response to continue. Notifications are always newest first. Each one carries its type
    /// and parameters, not text: the interface builds the sentence. entityDeleted and commentDeleted
    /// say that the task, project or comment it points to was deleted since.
    /// </remarks>
    /// <response code="200">Returns the notifications.</response>
    /// <response code="400">The paging values are invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(KeysetPagedResult<NotificationModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<KeysetPagedResult<NotificationModel>>> GetMany(
        [FromQuery] GetNotificationsRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(request, cancellationToken));
    }

    /// <summary>
    /// Returns the number of the current user's unread notifications, for the bell.
    /// </summary>
    /// <remarks>
    /// Read once when the page loads and again after the realtime connection comes back; between them
    /// notification.created and notification.read carry the new number.
    /// </remarks>
    /// <response code="200">Returns the unread count.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(NotificationSummaryModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<NotificationSummaryModel>> GetSummary(CancellationToken cancellationToken)
    {
        return Ok(await mediator.Send(new GetNotificationSummaryRequest(), cancellationToken));
    }

    /// <summary>
    /// Marks one of the current user's notifications as read.
    /// </summary>
    /// <remarks>
    /// Marking a read notification again changes nothing. The user's other tabs get notification.read
    /// with the new unread count.
    /// </remarks>
    /// <response code="204">The notification is read.</response>
    /// <response code="400">The id is empty, or there is no such notification among the user's own.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="404">The notification was deleted while the request was on its way.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkNotificationReadCommand { NotificationId = id }, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Marks one of the current user's notifications as unread again.
    /// </summary>
    /// <remarks>
    /// Marking an unread notification again changes nothing. The user's other tabs get notification.read
    /// with the new unread count.
    /// </remarks>
    /// <response code="204">The notification is unread.</response>
    /// <response code="400">The id is empty, or there is no such notification among the user's own.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="404">The notification was deleted while the request was on its way.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpPost("{id:guid}/unread")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorModel), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkUnread(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkNotificationUnreadCommand { NotificationId = id }, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Marks all of the current user's notifications as read.
    /// </summary>
    /// <remarks>
    /// The user's other tabs get notification.read with the current unread count.
    /// </remarks>
    /// <response code="204">All notifications are read.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="500">Unhandled server error</response>
    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorModel), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkAllNotificationsReadCommand(), cancellationToken);
        return NoContent();
    }
}
