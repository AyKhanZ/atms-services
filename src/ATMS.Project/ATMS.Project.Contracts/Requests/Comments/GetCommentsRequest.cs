using ATMS.Application.Security;
using ATMS.Contracts.Requests;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Comments;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ProjectView)]
public sealed class GetCommentsRequest : GetKeysetPaginationRequest,
    IRequest<KeysetPagedResult<CommentModel>>, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    /// <summary>Comments of this task or subtask.</summary>
    public Guid WorkTaskId { get; init; }
}
