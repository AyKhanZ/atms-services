namespace ATMS.Contracts.Events.Users;

public sealed record UserUpdatedEvent(
    Guid Id,
    string Name,
    string Surname,
    string AvatarPath);
