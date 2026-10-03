using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Project.Contracts.Commands.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Resources;
using MediatR;

namespace ATMS.Project.Services.Handlers.Notifications;

public sealed class MarkNotificationReadHandler(
    INotificationRepository notifications,
    ICurrentUser currentUser) : IRequestHandler<MarkNotificationReadCommand>
{
    public async Task Handle(MarkNotificationReadCommand command, CancellationToken cancellationToken)
    {
        var notification = await notifications.FindAsync(currentUser.Id, command.NotificationId, cancellationToken)
            ?? throw new EntityException(EntityErrorType.NotFound, NotificationMessages.NotFound);

        if (notification.ReadAt is not null)
        {
            return;
        }

        notification.ReadAt = DateTime.UtcNow;
        await notifications.SaveChangesAsync(cancellationToken);
    }
}
