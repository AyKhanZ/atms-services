namespace ATMS.Data;

public abstract class UserBase : BaseEntity
{
    public string Email { get; set; }

    public string Name { get; set; }
    
    public string Surname { get; set; }
}
