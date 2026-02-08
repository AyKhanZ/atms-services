namespace ATMS.Admin.Data.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<UserRole> UserRoles { get; set; }
    public List<RolePermission> RolePermissions { get; set; }
}
