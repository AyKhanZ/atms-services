using ATMS.Data;

namespace Project.Data.Entities;

public class Organization : BaseEntity
{
    public string Title { get; set; }
    
    public string Voen { get; set; }
    
    public string? LogoPath { get; set; }

    public ICollection<Project> Projects { get; set; } = [];
}
