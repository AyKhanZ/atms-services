using ATMS.Application.Enums;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Services.Domain.Security.Interfaces;

public interface IProjectAccessPolicyResolver
{
    Task<IReadOnlyCollection<ProjectPermissionEnum>> ResolveAsync(
        ProjectAccessPolicyEnum policy,
        IProjectScopedRequest request,
        CancellationToken cancellationToken);
}
