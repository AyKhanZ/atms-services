using System.Globalization;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Infrastructure;
using ATMS.Project.Services.Models.Notifications;
using ATMS.Project.Services.Domain.Notifications.Interfaces;

namespace ATMS.Project.Services.Domain.Notifications;

// DueToday on the deadline day, TaskOverdue after it; the dedup key (task + date) means no double reminders
public sealed class DeadlineNotificationService(
    INotificationRepository repository,
    INotificationService notifications,
    BusinessTimeZone businessTimeZone) : IDeadlineNotificationService
{
    // how far back missed days are caught up, more would spam people on the first day
    private const int OverdueCatchUpDays = 7;

    public async Task RemindAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        var today = businessTimeZone.Today(utcNow);

        var dueToday = await repository.GetOpenTasksDueAsync(
            businessTimeZone.StartOfDayUtc(today),
            businessTimeZone.StartOfDayUtc(today.AddDays(1)),
            cancellationToken);
        var overdue = await repository.GetOpenTasksDueAsync(
            businessTimeZone.StartOfDayUtc(today.AddDays(-OverdueCatchUpDays)),
            businessTimeZone.StartOfDayUtc(today),
            cancellationToken);

        var batch = dueToday
            .Select(task => new NotificationRecipients(
                Draft(NotificationTypeEnum.DueToday, "due-today", task),
                task.AssigneeUserId is { } assignee ? [assignee] : []))
            .Concat(overdue.Select(task => new NotificationRecipients(
                Draft(NotificationTypeEnum.TaskOverdue, "overdue", task),
                new Guid?[] { task.AssigneeUserId }
                    .Concat(task.ManagerUserIds.Cast<Guid?>())
                    .OfType<Guid>()
                    .ToArray())))
            .ToArray();

        await notifications.AddRangeAsync(batch, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private NotificationDraft Draft(NotificationTypeEnum type, string keyPrefix, DeadlineTaskRow task)
    {
        var deadline = businessTimeZone.DateOf(task.Deadline);
        return new NotificationDraft(
            type,
            task.ProjectId,
            NotificationEntityTypeEnum.WorkTask,
            task.Id,
            new NotificationParameters
            {
                ProjectTitle = task.ProjectTitle,
                TaskCode = task.Code,
                TaskTitle = task.Title,
                TaskKind = (int)(task.IsSubtask ? WorkTaskKindEnum.Subtask : WorkTaskKindEnum.Task),
                Deadline = deadline
            })
        {
            DedupKey = $"{keyPrefix}:{task.Id}:{deadline.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}"
        };
    }
}
