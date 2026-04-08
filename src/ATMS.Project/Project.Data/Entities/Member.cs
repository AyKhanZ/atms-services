using ATMS.Data;

namespace Project.Data.Entities;

public class Member : BaseEntity
{
    public Guid UserId { get; set; }
    
    public User User { get; set; }
    
    public string Name { get; set; }
    
    public string Surname { get; set; }
    
    public string Email { get; set; }
}
