using ATMS.Project.Contracts.Models.Users;

namespace ATMS.Project.Contracts.Models.Notifications;

public sealed class NotificationModel
{
    public Guid Id { get; set; }

    public int Type { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public PersonModel? Actor { get; set; }

    public Guid ProjectId { get; set; }

    public int EntityType { get; set; }

    public Guid EntityId { get; set; }

    public Guid? WorkTicketId { get; set; }

    public int? TaskStatusId { get; set; }

    public DateTime? TaskDeadline { get; set; }

    public Guid? CommentId { get; set; }

    public NotificationParametersModel Parameters { get; set; }

    public bool EntityDeleted { get; set; }

    public bool CommentDeleted { get; set; }
}
