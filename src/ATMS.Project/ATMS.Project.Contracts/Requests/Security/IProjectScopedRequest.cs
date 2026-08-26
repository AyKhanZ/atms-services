namespace ATMS.Project.Contracts.Requests.Security;

public interface IProjectScopedRequest
{
    Guid ProjectId { get; }
}
