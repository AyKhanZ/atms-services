using ATMS.Data.Interfaces;

namespace ATMS.Data;

public abstract class AuditableEntity<TUser> : BaseEntity, ICreatedByAuditable
{
    public Guid CreatedById { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedById { get; set; }

    public TUser CreatedBy { get; set; }

    public TUser UpdatedBy { get; set; }
}
