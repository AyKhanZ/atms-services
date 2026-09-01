namespace ATMS.Project.Contracts.Requests.Security;

public interface IProjectRoleScopedRequest : IProjectScopedRequest
{
    Guid RoleId { get; }
}
