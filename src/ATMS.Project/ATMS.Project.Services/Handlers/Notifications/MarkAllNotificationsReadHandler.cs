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

        // ExecuteUpdate skips the change tracker, so the count is pushed here after it
        // no cancellation: other tabs must learn it even if this one was closed
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
