namespace ATMS.Project.Contracts.Requests.Security;

public interface IProjectCommentScopedRequest : IProjectScopedRequest
{
    Guid CommentId { get; }
}
