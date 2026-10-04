using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Services.Models.Notifications;

public sealed record NotificationDraft(
    NotificationTypeEnum Type,
    Guid ProjectId,
    NotificationEntityTypeEnum EntityType,
    Guid EntityId,
    NotificationParameters Parameters)
{
    public Guid? ActorId { get; init; }

    public Guid? CommentId { get; init; }

    public string? DedupKey { get; init; }
}
