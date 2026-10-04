using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.Comments;

public sealed class CommentFilter : ACriteria<Comment>
{
    public Guid WorkTaskId { get; set; }

    public override IQueryable<Comment> Apply(IQueryable<Comment> query) =>
        query.Where(comment =>
            comment.OwnerType == (int)CommentOwnerTypeEnum.Task &&
            comment.OwnerId == WorkTaskId);
}
