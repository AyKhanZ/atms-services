using ATMS.Application.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Services.Models.Notifications;
using ATMS.Project.Services.Domain.Notifications.Interfaces;

namespace ATMS.Project.Services.Domain.Notifications;

public sealed class WorkProjectNotificationService(
    INotificationService notifications,
    ICurrentUser currentUser) : IWorkProjectNotificationService
{
    public Task NotifyParticipantsAddedAsync(
        WorkProject project,
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken) =>
        notifications.AddAsync(
            new NotificationDraft(
                NotificationTypeEnum.AddedToProject,
                project.Id,
                NotificationEntityTypeEnum.Project,
                project.Id,
                new NotificationParameters { ProjectTitle = project.Title })
            {
                ActorId = currentUser.Id
            },
            userIds,
            cancellationToken);
}
