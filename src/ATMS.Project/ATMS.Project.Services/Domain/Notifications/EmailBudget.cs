using ATMS.Project.Data.Entities;

namespace ATMS.Project.Services.Domain.Notifications;

// which emails of this batch still fit today; the caller has already read the counts
internal static class EmailBudget
{
    internal readonly record struct Result(
        IReadOnlyList<Notification> Accepted,
        int SkippedForUser,
        bool ServiceLimitReached);

    internal static Result Take(
        IReadOnlyList<Notification> ordered,
        int sentToday,
        IReadOnlyDictionary<Guid, int> sentByUser,
        int maxPerUser,
        int maxPerDay)
    {
        var accepted = new List<Notification>(ordered.Count);
        var takenByUser = new Dictionary<Guid, int>();
        var skippedForUser = 0;
        var serviceCount = sentToday;

        foreach (var notification in ordered)
        {
            var already = sentByUser.GetValueOrDefault(notification.UserId)
                + takenByUser.GetValueOrDefault(notification.UserId);
            if (already >= maxPerUser)
            {
                skippedForUser++;
                continue;
            }

            if (serviceCount >= maxPerDay)
            {
                continue;
            }

            accepted.Add(notification);
            takenByUser[notification.UserId] = takenByUser.GetValueOrDefault(notification.UserId) + 1;
            serviceCount++;
        }

        return new Result(
            accepted,
            skippedForUser,
            sentToday < maxPerDay && serviceCount >= maxPerDay);
    }
}
