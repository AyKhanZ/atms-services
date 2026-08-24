namespace ATMS.Contracts.Events.Users;

public sealed record UserCreatedEvent(
    Guid Id,
    string Email,
    string Name,
    string Surname,
    int UserType,
    string AvatarPath,
    Guid? OrganizationId,
    bool IsAdmin = false);
