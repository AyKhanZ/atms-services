using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Exceptions.Resources;
using ATMS.Data.Enums;
using ATMS.Infrastructure.Options;
using ATMS.Project.Data.Criteria.Users;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Notifications;
using ATMS.Project.Data.Models.WorkProjects;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Infrastructure;
using ATMS.Project.Services.Models.Notifications;
using ATMS.Project.Services.Domain.Notifications.Interfaces;

namespace ATMS.Project.Services.Domain.Notifications;

// nothing is saved here, it goes out with the caller's SaveChanges
public sealed class NotificationService(
    INotificationRepository notifications,
    IEmailDeliveryRepository emails,
    IProjectPermissionRepository permissions,
    IUserRepository users,
    IOptions<NotificationsOptions> notificationsOptions,
    BusinessTimeZone businessTimeZone,
    ILogger<NotificationService> logger) : INotificationService
{
    private static readonly TimeSpan MergeWindow = TimeSpan.FromMinutes(10);

    // only things the person has to act on go by email, the rest would be noise
    private static readonly HashSet<int> EmailedTypes =
    [
        (int)NotificationTypeEnum.TaskAssigned,
        (int)NotificationTypeEnum.Mentioned,
        (int)NotificationTypeEnum.DueToday,
        (int)NotificationTypeEnum.TaskOverdue,
        (int)NotificationTypeEnum.AddedToProject
    ];

    private readonly NotificationsOptions _options = notificationsOptions.Value;

    public Task AddAsync(
        NotificationDraft draft,
        IEnumerable<Guid> recipients,
        CancellationToken cancellationToken) =>
        AddRangeAsync([new NotificationRecipients(draft, recipients.ToArray())], cancellationToken);

    public async Task AddRangeAsync(
        IReadOnlyCollection<NotificationRecipients> batch,
        CancellationToken cancellationToken)
    {
        var pending = batch
            .SelectMany(item => item.UserIds
                .Where(userId => userId != Guid.Empty && userId != item.Draft.ActorId)
                .Distinct()
                .Select(userId => (item.Draft, UserId: userId)))
            .ToList();
        if (pending.Count == 0)
        {
            return;
        }

        await KeepProjectViewersAsync(pending, cancellationToken);
        await DropSentRemindersAsync(pending, cancellationToken);

        var now = DateTime.UtcNow;
        await MergeIntoUnreadAsync(pending, now, cancellationToken);
        if (pending.Count == 0)
        {
            return;
        }

        var created = pending
            .Select(item => new Notification
            {
                Id = Guid.NewGuid(),
                UserId = item.UserId,
                Type = (int)item.Draft.Type,
                WorkProjectId = item.Draft.ProjectId,
                EntityType = (int)item.Draft.EntityType,
                EntityId = item.Draft.EntityId,
                CommentId = item.Draft.CommentId,
                ActorId = item.Draft.ActorId,
                Parameters = item.Draft.Parameters with { },
                DedupKey = item.Draft.DedupKey,
                CreatedAt = now
            })
            .ToArray();
        await notifications.AddRangeAsync(created, cancellationToken);

        if (_options.SendEmails)
        {
            var emailable = created.Where(notification => EmailedTypes.Contains(notification.Type)).ToArray();
            if (emailable.Length == 0)
            {
                return;
            }

            // an inactive person still gets the bell, just no email
            var activeUserIds = (await users.GetManyAsync(
                    emailable.Select(notification => notification.UserId).Distinct(),
                    new ActiveUsersCriteria(),
                    cancellationToken))
                .Select(user => user.Id)
                .ToHashSet();
            var toSend = emailable.Where(notification => activeUserIds.Contains(notification.UserId)).ToArray();
            if (toSend.Length == 0)
            {
                return;
            }

            var startOfDay = businessTimeZone.StartOfDayUtc(businessTimeZone.Today(now));
            var sentToday = await emails.CountSinceAsync(startOfDay, cancellationToken);
            var sentByUser = await emails.CountByUserSinceAsync(
                toSend.Select(notification => notification.UserId).Distinct().ToArray(),
                startOfDay,
                cancellationToken);
            var budget = EmailBudget.Take(
                toSend,
                sentToday,
                sentByUser,
                _options.MaxEmailsPerUserPerDay,
                _options.MaxEmailsPerDay);

            if (budget.SkippedForUser > 0)
            {
                logger.LogInformation(
                    "{Count} notification emails skipped, daily limit per recipient",
                    budget.SkippedForUser);
            }

            if (budget.ServiceLimitReached)
            {
                logger.LogWarning("Daily email limit of the service reached");
            }

            if (budget.Accepted.Count == 0)
            {
                return;
            }

            await emails.AddRangeAsync(
                budget.Accepted.Select(notification => new EmailDelivery
                {
                    Id = Guid.NewGuid(),
                    NotificationId = notification.Id,
                    Status = (int)DeliveryStatusEnum.Pending,
                    CreatedAt = now,
                    NextAttemptAt = now
                }),
                cancellationToken);
        }
    }

    // a person added in the same save has no access in the db yet, and every project role has Project view
    private async Task KeepProjectViewersAsync(
        List<(NotificationDraft Draft, Guid UserId)> pending,
        CancellationToken cancellationToken)
    {
        var toCheck = pending.Where(item => item.Draft.Type != NotificationTypeEnum.AddedToProject).ToArray();
        if (toCheck.Length == 0)
        {
            return;
        }

        var viewers = (await permissions.GetUsersWithPermissionAsync(
                toCheck.Select(item => item.Draft.ProjectId).Distinct().ToArray(),
                toCheck.Select(item => item.UserId).Distinct().ToArray(),
                ProjectPermissionEnum.ProjectView,
                cancellationToken))
            .ToHashSet();

        pending.RemoveAll(item =>
            item.Draft.Type != NotificationTypeEnum.AddedToProject &&
            !viewers.Contains(new ProjectUserRow(item.Draft.ProjectId, item.UserId)));
    }

    private async Task DropSentRemindersAsync(
        List<(NotificationDraft Draft, Guid UserId)> pending,
        CancellationToken cancellationToken)
    {
        var keyed = pending.Where(item => item.Draft.DedupKey is not null).ToArray();
        if (keyed.Length == 0)
        {
            return;
        }

        var sent = (await notifications.GetDedupKeysAsync(
                keyed.Select(item => item.UserId).Distinct().ToArray(),
                keyed.Select(item => item.Draft.DedupKey!).Distinct().ToArray(),
                cancellationToken))
            .ToHashSet();

        pending.RemoveAll(item =>
            item.Draft.DedupKey is { } key && sent.Contains(new NotificationKeyRow(item.UserId, key)));
    }

    // only the newest unread row takes the news; tasks are locked first, so parallel changes merge
    private async Task MergeIntoUnreadAsync(
        List<(NotificationDraft Draft, Guid UserId)> pending,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var mergeable = pending.Where(item => item.Draft.DedupKey is null).ToArray();
        if (mergeable.Length == 0)
        {
            return;
        }

        var entityIds = mergeable.Select(item => item.Draft.EntityId).Distinct().ToArray();
        await notifications.LockForMergeAsync(entityIds, cancellationToken);
        var unread = await notifications.GetUnreadSinceAsync(
            mergeable.Select(item => item.UserId).Distinct().ToArray(),
            entityIds,
            now - MergeWindow,
            cancellationToken);
        var newest = unread
            .GroupBy(notification => (notification.UserId, notification.Type, notification.EntityId))
            .ToDictionary(group => group.Key, group => group.MaxBy(notification => notification.CreatedAt)!);

        foreach (var item in mergeable)
        {
            if (!newest.Remove((item.UserId, (int)item.Draft.Type, item.Draft.EntityId), out var existing))
            {
                continue;
            }

            existing.CreatedAt = now;
            existing.ActorId = item.Draft.ActorId;
            existing.CommentId = item.Draft.CommentId;
            existing.Parameters = item.Draft.Parameters with { };
            pending.Remove(item);
        }
    }
}
