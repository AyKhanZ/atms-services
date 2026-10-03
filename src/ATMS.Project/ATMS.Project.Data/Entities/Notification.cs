using ATMS.Data;

namespace ATMS.Project.Data.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }

    public int Type { get; set; }

    public Guid WorkProjectId { get; set; }

    public int EntityType { get; set; }

    public Guid EntityId { get; set; }

    public Guid? CommentId { get; set; }

    public Guid? ActorId { get; set; }

    public User? Actor { get; set; }

    public NotificationParameters Parameters { get; set; }

    public string? DedupKey { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }
}
