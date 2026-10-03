using ATMS.Project.Data.Entities;

namespace ATMS.Project.Services.Notifications.Interfaces;

public interface ICommentNotificationService
{
    Task NotifyCreatedAsync(Guid projectId, Comment comment, CancellationToken cancellationToken);

    Task NotifyEditedAsync(
        Guid projectId,
        Comment comment,
        string previousText,
        CancellationToken cancellationToken);
}
