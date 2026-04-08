using ATMS.Data;

namespace Project.Data.Entities;

public class User : BaseEntity
{
    public string Name { get; set; }
    
    public string Surname { get; set; }
    
    public string Email { get; set; }
}
