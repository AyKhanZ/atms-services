using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.History;

namespace ATMS.Project.Data.Models.Notifications;

public sealed class NotificationRow
{
    public Guid Id { get; init; }

    public int Type { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? ReadAt { get; init; }

    public HistoryPerson? Actor { get; init; }

    public Guid WorkProjectId { get; init; }

    public int EntityType { get; init; }

    public Guid EntityId { get; init; }

    public Guid? WorkTicketId { get; init; }

    public Guid? CommentId { get; init; }

    public NotificationParameters Parameters { get; init; }

    public bool EntityDeleted { get; init; }

    public bool CommentDeleted { get; init; }
}
