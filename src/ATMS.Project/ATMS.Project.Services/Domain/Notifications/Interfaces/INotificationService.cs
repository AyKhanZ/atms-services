using ATMS.Project.Services.Models.Notifications;

namespace ATMS.Project.Services.Domain.Notifications.Interfaces;

public interface INotificationService
{
    Task AddAsync(
        NotificationDraft draft,
        IEnumerable<Guid> recipients,
        CancellationToken cancellationToken);

    Task AddRangeAsync(
        IReadOnlyCollection<NotificationRecipients> batch,
        CancellationToken cancellationToken);
}
