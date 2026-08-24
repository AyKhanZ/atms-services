using ATMS.Data;
using ATMS.Data.Enums;

namespace ATMS.Project.Data.Entities;

public class Attachment : SoftDeletableAuditableEntity<User>
{
    public AttachmentOwnerTypeEnum OwnerType { get; set; }

    public Guid OwnerId { get; set; }

    public string FileName { get; set; }

    public string RelativePath { get; set; }

    public string ContentType { get; set; }

    public long Size { get; set; }

    public Guid? CommentId { get; set; }

    public Comment? Comment { get; set; }
}
