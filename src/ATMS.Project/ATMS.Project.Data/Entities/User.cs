using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Project.Data.Entities;

public class User : UserBase, ISoftDeletable
{
    public int UserTypeId { get; set; }

    
    public Guid? OrganizationId { get; set; }
    
    public Organization? Organization { get; set; }
    
    
    public bool IsDeleted { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public Guid? DeletedById { get; set; }
}
