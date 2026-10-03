using ATMS.Project.Data.Entities;

namespace ATMS.Project.Services.Notifications.Interfaces;

public interface IWorkTaskNotificationService
{
    Task NotifyChangedAsync(
        WorkTask workTask,
        Guid? previousAssigneeId,
        int? previousStatusId,
        CancellationToken cancellationToken);
}
