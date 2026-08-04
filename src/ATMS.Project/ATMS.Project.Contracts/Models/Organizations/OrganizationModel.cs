using ATMS.Project.Contracts.Models.Users;

namespace ATMS.Project.Contracts.Models.Organizations;

public class OrganizationModel
{
    public Guid Id { get; set; }
    
    public string Title { get; set; }
    
    public string Voen { get; set; }
    
    public string LogoPath { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public UserModel[] Users { get; set; } = [];
}
