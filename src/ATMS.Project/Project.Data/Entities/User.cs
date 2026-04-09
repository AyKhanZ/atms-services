using ATMS.Data;

namespace Project.Data.Entities;

public class User : BaseEntity
{
    public string Name { get; set; }
    
    public string Surname { get; set; }
    
    public string Email { get; set; }
    
    
    public uint UserType { get; set; }
    
    // public UserType UserType { get; set; }

    
    public Guid? OrganizationId { get; set; }
    
    public Organization? Organization { get; set; }
}
