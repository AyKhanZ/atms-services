namespace ATMS.Project.Data.Models.Notifications;

// The recipient is null when the person was deleted after the notification was written.
public sealed record EmailDeliveryRow(
    Guid Id,
    int Status,
    Guid NotificationId,
    Guid RecipientUserId,
    string? RecipientEmail,
    string? RecipientName,
    string? RecipientSurname);
