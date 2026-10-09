namespace ATMS.Contracts.Events.Users;

public sealed record UserStatusChangedEvent(Guid Id, bool IsActive);
