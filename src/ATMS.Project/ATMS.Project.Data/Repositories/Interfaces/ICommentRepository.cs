using ATMS.Data.Criteria;
using ATMS.Data.Criteria.Interfaces;
using ATMS.Project.Data.Entities;
using ATMS.Project.Data.Models.Comments;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface ICommentRepository
{
    Task<KeysetPagedResult<Comment>> GetManyAsync(
        Guid projectId,
        ACriteria<Comment> criteria,
        KeysetPaginationCriteria<Comment> pagination,
        CancellationToken cancellationToken);

    Task<Comment?> GetAsync(Guid projectId, Guid commentId, CancellationToken cancellationToken);

    Task<Comment?> FindAsync(Guid projectId, Guid commentId, CancellationToken cancellationToken);

    Task<Guid?> GetAuthorIdAsync(Guid projectId, Guid commentId, CancellationToken cancellationToken);

    Task<bool> IsLiveCommentAsync(Guid projectId, Guid commentId, CancellationToken cancellationToken);

    Task<bool> IsOwnerTaskLiveAsync(Guid projectId, Guid workTaskId, CancellationToken cancellationToken);

    Task<int> CountAsync(Guid workTaskId, CancellationToken cancellationToken);

    Task<User[]> GetAuthorsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken);

    Task<User[]> GetMentionedParticipantsAsync(
        Guid projectId,
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken);

    Task<CommentWorkItemReferenceRow[]> GetReferencesAsync(
        IReadOnlyCollection<string> codes,
        ICriteria<WorkProject> accessibleProjects,
        CancellationToken cancellationToken);

    Task AddAsync(Comment comment, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
