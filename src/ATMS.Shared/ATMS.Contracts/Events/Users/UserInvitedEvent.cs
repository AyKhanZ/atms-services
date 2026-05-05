namespace ATMS.Contracts.Events.Users;

public sealed record UserInvitedEvent(
    string Email,
    string Name,
    string Surname,
    Guid? OrganizationId,
    Guid InvitedByUserId);