namespace ATMS.Application.Interfaces;

public interface ICurrentUser
{
    Guid Id { get; }
    Guid RoleId { get; }
    Guid? OrganizationId { get; }
    string UserType { get; }
}
