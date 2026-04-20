namespace ATMS.Contracts.Events.Users;

public sealed record UserCreatedEvent(
    Guid Id,
    string Email,
    string Name,
    string Surname,
    int UserType,
    Guid? OrganizationId);