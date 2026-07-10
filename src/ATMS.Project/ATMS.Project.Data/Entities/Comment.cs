using ATMS.Data;
using ATMS.Data.Enums;
using ATMS.Data.Interfaces;

namespace ATMS.Project.Data.Entities;

public class Comment : AuditableEntity, ISoftDeletable
{
    public CommentOwnerTypeEnum OwnerType { get; set; }

    public Guid OwnerId { get; set; }

    public string Text { get; set; }

    public Guid? ParentCommentId { get; set; }

    public Comment? ParentComment { get; set; }

    public ICollection<Comment> Replies { get; set; } = [];

    public ICollection<Attachment> Attachments { get; set; } = [];

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedById { get; set; }
}
