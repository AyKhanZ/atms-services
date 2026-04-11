using ATMS.Data;

namespace ATMS.Admin.Data.Entities.UserProgress;

public class OrganizationInfo : BaseEntity
{
    public string Name { get; set; }
    
    public string Voen { get; set; }
    
    public string LogoImagePath { get; set; }

    public Guid UserProgressId { get; set; }
    
    public UserProgress UserProgress { get; set; }
}
