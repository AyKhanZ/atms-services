using ATMS.Application.Security;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using ATMS.Project.Services.Security.Interfaces;

namespace ATMS.Project.Services.Security;

public sealed class ProjectAccessPolicyResolver : IProjectAccessPolicyResolver
{
    public Task<IReadOnlyCollection<ProjectPermissionEnum>> ResolveAsync(
        ProjectAccessPolicy policy,
        IProjectScopedRequest request,
        CancellationToken cancellationToken)
    {
        var permissions = policy switch
        {
            ProjectAccessPolicy.ParticipantInvite => ResolveParticipantInvite(request),
            _ => []
        };

        return Task.FromResult(permissions);
    }

    private static IReadOnlyCollection<ProjectPermissionEnum> ResolveParticipantInvite(IProjectScopedRequest request)
    {
        if (request is not IProjectRoleScopedRequest roleRequest)
        {
            return [];
        }

        return IsClientRole(roleRequest.RoleId)
            ? [ProjectPermissionEnum.ParticipantInviteClient]
            : [ProjectPermissionEnum.ParticipantInviteEmployee];
    }

    private static bool IsClientRole(Guid roleId)
    {
        return roleId == RoleIds.OrgClientManager ||
               roleId == RoleIds.OrgClientViewer;
    }
}
