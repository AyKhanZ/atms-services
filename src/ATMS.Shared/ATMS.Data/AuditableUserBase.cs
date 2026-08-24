using ATMS.Data.Interfaces;

namespace ATMS.Data;

public abstract class AuditableUserBase : UserAccountBase, IAuditable
{
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedById { get; set; }
}
