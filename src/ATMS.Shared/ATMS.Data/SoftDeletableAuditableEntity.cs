using ATMS.Data.Interfaces;

namespace ATMS.Data;

public abstract class SoftDeletableAuditableEntity<TUser> : AuditableEntity<TUser>, ISoftDeletable
{
    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedById { get; set; }

    public TUser DeletedBy { get; set; }
}
