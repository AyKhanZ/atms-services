using ATMS.Project.Data.Entities;

namespace ATMS.Project.Services.Domain.Notifications.Interfaces;

public interface IWorkProjectNotificationService
{
    Task NotifyParticipantsAddedAsync(
        WorkProject project,
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken);
}
