using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Commands.Comments;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.CommentEdit)]
public sealed class UpdateCommentCommand : IRequest<CommentModel>, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    public Guid CommentId { get; set; }

    public required string Text { get; set; }
}
