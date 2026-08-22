using ATMS.Data.Interfaces;

namespace ATMS.Data;

public abstract class AuditableEntity : BaseEntity, IAuditable
{
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedById { get; set; }
}
