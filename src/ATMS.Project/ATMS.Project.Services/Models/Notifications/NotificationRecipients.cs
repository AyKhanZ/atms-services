namespace ATMS.Project.Services.Models.Notifications;

public sealed record NotificationRecipients(NotificationDraft Draft, IReadOnlyCollection<Guid> UserIds);
