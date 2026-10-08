using ATMS.Application.Enums;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Commands.Comments;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectAccessPolicyEnum.CommentDelete)]
public sealed class DeleteCommentCommand : IRequest, IProjectCommentScopedRequest
{
    public Guid ProjectId { get; set; }

    public Guid CommentId { get; set; }
}
