namespace ATMS.Data.Interfaces;

public interface IAuditable
{
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedById { get; set; }
}
