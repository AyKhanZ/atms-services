namespace ATMS.Application.Realtime;

public sealed record NotificationCreatedEvent(Guid Id, int UnreadCount);
