using ATMS.Data;

namespace Project.Data.Entities;

public class Group : BaseEntity
{
    public string Title { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    
    public Guid ProjectId { get; set; }
    
    public Project Project { get; set; }
}
