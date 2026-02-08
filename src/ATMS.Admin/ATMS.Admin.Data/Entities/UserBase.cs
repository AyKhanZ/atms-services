namespace ATMS.Admin.Data.Entities;

public class UserBase : BaseEntity
{
    public string Email { get; set; }

    public string Name { get; set; }
    public string Surname { get; set; }
}