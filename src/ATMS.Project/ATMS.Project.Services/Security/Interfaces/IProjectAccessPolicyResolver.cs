using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Services.Security.Interfaces;

public interface IProjectAccessPolicyResolver
{
    Task<IReadOnlyCollection<ProjectPermissionEnum>> ResolveAsync(
        ProjectAccessPolicy policy,
        IProjectScopedRequest request,
        CancellationToken cancellationToken);
}
