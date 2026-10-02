using ATMS.Data;
using ATMS.Data.Enums;

namespace ATMS.Project.Data.Entities;

public class Comment : SoftDeletableAuditableEntity<User>
{
    public CommentOwnerTypeEnum OwnerType { get; set; }

    public Guid OwnerId { get; set; }

    public string Text { get; set; }

    public ICollection<Attachment> Attachments { get; set; } = [];
}
