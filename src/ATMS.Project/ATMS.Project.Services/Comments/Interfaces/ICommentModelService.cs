using ATMS.Project.Contracts.Models.Comments;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Services.Comments.Interfaces;

public interface ICommentModelService
{
    Task<IReadOnlyDictionary<Guid, CommentModel>> BuildAsync(
        Guid projectId,
        IReadOnlyCollection<Comment> comments,
        CancellationToken cancellationToken);
}
