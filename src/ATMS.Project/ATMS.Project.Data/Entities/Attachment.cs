using ATMS.Data;
using ATMS.Data.Enums;
using ATMS.Data.Interfaces;

namespace ATMS.Project.Data.Entities;

public class Attachment : AuditableEntity, ISoftDeletable
{
    public AttachmentOwnerTypeEnum OwnerType { get; set; }

    public Guid OwnerId { get; set; }

    public string FileName { get; set; }

    public string RelativePath { get; set; }

    public string ContentType { get; set; }

    public long Size { get; set; }

    public Guid? CommentId { get; set; }

    public Comment? Comment { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedById { get; set; }
}
