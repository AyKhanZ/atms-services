using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.Notifications;

public sealed class NotificationFilter : ACriteria<Notification>
{
    public bool UnreadOnly { get; set; }

    public override IQueryable<Notification> Apply(IQueryable<Notification> query) =>
        UnreadOnly ? query.Where(notification => notification.ReadAt == null) : query;
}
