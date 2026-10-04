using ATMS.Application.Interfaces;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Models.Notifications;
using ATMS.Project.Services.Notifications.Interfaces;

namespace ATMS.Project.Services.Notifications;

// Who hears that a task was assigned or moved. Called by the handlers for the task the person acted
// on, not for the subtasks closed along with it: one action, one notification.
public sealed class WorkTaskNotificationService(
    INotificationRepository repository,
    INotificationService notifications,
    ICurrentUser currentUser) : IWorkTaskNotificationService
{
    public async Task NotifyChangedAsync(
        WorkTask workTask,
        Guid? previousAssigneeId,
        int? previousStatusId,
        CancellationToken cancellationToken)
    {
        var assigned = workTask.AssigneeId.HasValue && workTask.AssigneeId != previousAssigneeId;
        var statusChanged = previousStatusId.HasValue && previousStatusId != workTask.StatusId;
        if (!assigned && !statusChanged)
        {
            return;
        }

        var audience = await repository.GetWorkTaskAudienceAsync(
            workTask.WorkProjectId,
            workTask.AssigneeId,
            cancellationToken);
        if (audience is null)
        {
            return;
        }

        var parameters = new NotificationParameters
        {
            ProjectTitle = audience.ProjectTitle,
            TaskCode = workTask.Code,
            TaskTitle = workTask.Title,
            TaskKind = (int)(workTask.ParentWorkTaskId.HasValue ? WorkTaskKindEnum.Subtask : WorkTaskKindEnum.Task)
        };
        var batch = new List<NotificationRecipients>();

        if (assigned && audience.AssigneeUserId is { } assigneeUserId)
        {
            batch.Add(new NotificationRecipients(
                Draft(NotificationTypeEnum.TaskAssigned, workTask, parameters),
                [assigneeUserId]));
        }

        if (statusChanged)
        {
            // A new assignee already hears about the task from TaskAssigned.
            Guid?[] recipients = [assigned ? null : audience.AssigneeUserId, workTask.CreatedById];
            batch.Add(new NotificationRecipients(
                Draft(
                    NotificationTypeEnum.TaskStatusChanged,
                    workTask,
                    parameters with { FromStatusId = previousStatusId, ToStatusId = workTask.StatusId }),
                recipients.OfType<Guid>().ToArray()));
        }

        await notifications.AddRangeAsync(batch, cancellationToken);
    }

    private NotificationDraft Draft(
        NotificationTypeEnum type,
        WorkTask workTask,
        NotificationParameters parameters) =>
        new(type, workTask.WorkProjectId, NotificationEntityTypeEnum.WorkTask, workTask.Id, parameters)
        {
            ActorId = currentUser.Id
        };
}
