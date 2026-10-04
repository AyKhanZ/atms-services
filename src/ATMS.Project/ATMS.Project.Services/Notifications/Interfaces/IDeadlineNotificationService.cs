namespace ATMS.Project.Services.Notifications.Interfaces;

public interface IDeadlineNotificationService
{
    Task RemindAsync(DateTime utcNow, CancellationToken cancellationToken);
}
