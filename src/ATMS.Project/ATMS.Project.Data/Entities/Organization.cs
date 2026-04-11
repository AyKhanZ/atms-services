using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Project.Data.Entities;

public class Organization : BaseEntity, ISoftDeletable
{
    public string Title { get; set; }
    
    public string Voen { get; set; }
    
    public string? LogoPath { get; set; }

    
    public ICollection<WorkProject> WorkProjects { get; set; } = [];
    
    public ICollection<User> Users { get; set; } = [];

    
    public bool IsDeleted { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public Guid? DeletedById { get; set; }
}
