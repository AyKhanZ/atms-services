namespace ATMS.Data.Interfaces;

public interface ISoftDeletable
{
    public bool IsDeleted { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public Guid? DeletedById { get; set; }
}
