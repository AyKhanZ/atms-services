namespace ATMS.Application.Interfaces;

public interface ICurrentUser
{
    Guid Id { get; }
    Guid RoleId { get; }
    IReadOnlySet<string> Permissions { get; }
    Guid? OrganizationId { get; }
    string UserType { get; }
}
