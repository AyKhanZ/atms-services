namespace ATMS.Project.Data.Models.Notifications;

// Recipient is null if the user was deleted after the notification
public sealed record EmailDeliveryRow(
    Guid Id,
    int Status,
    Guid NotificationId,
    Guid RecipientUserId,
    string? RecipientEmail,
    string? RecipientName,
    string? RecipientSurname,
    bool RecipientIsActive);
