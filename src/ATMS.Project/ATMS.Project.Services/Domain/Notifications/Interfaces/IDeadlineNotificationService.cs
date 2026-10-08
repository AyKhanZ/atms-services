namespace ATMS.Project.Services.Domain.Notifications.Interfaces;

public interface IDeadlineNotificationService
{
    Task RemindAsync(DateTime utcNow, CancellationToken cancellationToken);
}
