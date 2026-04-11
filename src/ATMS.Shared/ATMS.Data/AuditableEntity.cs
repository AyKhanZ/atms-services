namespace ATMS.Data;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public Guid CreatedById { get; set; }
}
