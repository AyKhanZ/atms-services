using ATMS.Application.Interfaces;
using ATMS.Application.Realtime;
using ATMS.Project.Contracts.Commands.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ATMS.Project.Services.Handlers.Notifications;

public sealed class MarkAllNotificationsReadHandler(
    INotificationRepository notifications,
    ICurrentUser currentUser,
    IRealtimeEventPublisher publisher,
    ILogger<MarkAllNotificationsReadHandler> logger) : IRequestHandler<MarkAllNotificationsReadCommand>
{
    public async Task Handle(MarkAllNotificationsReadCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        var updated = await notifications.MarkAllReadAsync(userId, DateTime.UtcNow, cancellationToken);
        if (updated == 0)
        {
            return;
        }

        // ExecuteUpdate bypasses the change tracker, so publish the count after the update commits.
        // Not cancelled with the request: the rows are already read, and the person's other tabs must
        // learn it even when this tab closed right after the click.
        try
        {
            var unreadCount = await notifications.CountUnreadAsync(userId, CancellationToken.None);
            await publisher.PublishToUsersAsync(
                [userId],
                RealtimeEventNames.NotificationRead,
                new NotificationReadEvent(unreadCount),
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Realtime push failed after marking all notifications read for user {UserId}", userId);
        }
    }
}
