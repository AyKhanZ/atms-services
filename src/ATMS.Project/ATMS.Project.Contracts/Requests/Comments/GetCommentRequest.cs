using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Comments;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ProjectView)]
public sealed class GetCommentRequest : IRequest<CommentModel>, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    public Guid CommentId { get; set; }
}
