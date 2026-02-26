using ATMS.Admin.Data.Entities.Dictionaries;

namespace ATMS.Admin.Data.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; }
    public int PermissionId { get; set; }
    public Permission Permission { get; set; }
}