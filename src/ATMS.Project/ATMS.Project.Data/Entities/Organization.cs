using ATMS.Data;

namespace ATMS.Project.Data.Entities;

public class Organization : SoftDeletableAuditableEntity<User>
{
    public string Title { get; set; }
    
    public string Voen { get; set; }
    
    public string? LogoPath { get; set; }

    
    public ICollection<WorkProject> WorkProjects { get; set; } = [];
    
    public ICollection<User> Users { get; set; } = [];
}
