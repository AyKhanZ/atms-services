using System.Globalization;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Time;
using ATMS.Project.Services.Models.Notifications;
using ATMS.Project.Services.Notifications.Interfaces;

namespace ATMS.Project.Services.Notifications;

// One pass of deadline reminders: DueToday on the day of the deadline, TaskOverdue on the days after
// it. A deadline is a date, read in the business time zone. Every reminder carries a key made of the
// task and the date, so a second pass, a second instance or a restart sends nothing twice, and a
// moved deadline is a new date and a new reminder.
public sealed class DeadlineNotificationService(
    INotificationRepository repository,
    INotificationService notifications,
    BusinessTimeZone businessTimeZone) : IDeadlineNotificationService
{
    // How far back a missed pass is caught up. Further back would bury people in old work the day
    // reminders are switched on.
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

        // The whole pass is one batch: the same few queries for a hundred tasks as for one.
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
