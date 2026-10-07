using ATMS.Application.Interfaces;
using ATMS.Application.Security;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Security.Interfaces;

namespace ATMS.Project.Services.Security;

public sealed class ProjectAccessPolicyResolver(
    ICommentRepository comments,
    IWorkProjectRepository workProjects,
    ICurrentUser currentUser) : IProjectAccessPolicyResolver
{
    public async Task<IReadOnlyCollection<ProjectPermissionEnum>> ResolveAsync(
        ProjectAccessPolicy policy,
        IProjectScopedRequest request,
        CancellationToken cancellationToken)
    {
        return policy switch
        {
            ProjectAccessPolicy.ParticipantInvite => ResolveParticipantInvite(request),
            ProjectAccessPolicy.CommentDelete => await ResolveCommentDeleteAsync(request, cancellationToken),
            ProjectAccessPolicy.ParticipantDelete => await ResolveParticipantDeleteAsync(request, cancellationToken),
            _ => []
        };
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

    // One's own comment goes with the right to write; someone else's needs Comment delete. A comment
    // that is not there asks for the right to write only, so the validator answers "not found".
    private async Task<IReadOnlyCollection<ProjectPermissionEnum>> ResolveCommentDeleteAsync(
        IProjectScopedRequest request,
        CancellationToken cancellationToken)
    {
        if (request is not IProjectCommentScopedRequest commentRequest)
        {
            return [];
        }

        var authorId = await comments.GetAuthorIdAsync(
            commentRequest.ProjectId,
            commentRequest.CommentId,
            cancellationToken);

        return authorId is null || authorId == currentUser.Id
            ? [ProjectPermissionEnum.CommentEdit]
            : [ProjectPermissionEnum.CommentDelete];
    }

    private async Task<IReadOnlyCollection<ProjectPermissionEnum>> ResolveParticipantDeleteAsync(
        IProjectScopedRequest request,
        CancellationToken cancellationToken)
    {
        if (request is not IProjectParticipantScopedRequest participantRequest)
        {
            return [];
        }

        var roleId = await workProjects.GetParticipantRoleIdAsync(
            participantRequest.ProjectId,
            participantRequest.ParticipantId,
            cancellationToken);

        return roleId is { } clientRoleId && IsClientRole(clientRoleId)
            ? [ProjectPermissionEnum.ParticipantDelete, ProjectPermissionEnum.ParticipantDeleteClient]
            : [ProjectPermissionEnum.ParticipantDelete];
    }

    private static bool IsClientRole(Guid roleId)
    {
        return roleId == RoleIds.OrgClientManager ||
               roleId == RoleIds.OrgClientViewer;
    }
}
