using ATMS.Application.Enums;
using ATMS.Application.Interfaces;
using ATMS.Application.Security;
using ATMS.Data.Constants;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.Security.Interfaces;

namespace ATMS.Project.Services.Domain.Security;

public sealed class ProjectAccessPolicyResolver(
    ICommentRepository comments,
    IWorkProjectRepository workProjects,
    ICurrentUser currentUser) : IProjectAccessPolicyResolver
{
    public async Task<IReadOnlyCollection<ProjectPermissionEnum>> ResolveAsync(
        ProjectAccessPolicyEnum policy,
        IProjectScopedRequest request,
        CancellationToken cancellationToken)
    {
        return policy switch
        {
            ProjectAccessPolicyEnum.ParticipantInvite => ResolveParticipantInvite(request),
            ProjectAccessPolicyEnum.CommentDelete => await ResolveCommentDeleteAsync(request, cancellationToken),
            ProjectAccessPolicyEnum.ParticipantDelete => await ResolveParticipantDeleteAsync(request, cancellationToken),
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

    // own comment needs the write right, someone else's needs CommentDelete
    // a missing comment asks for write only, so the validator says "not found"
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
