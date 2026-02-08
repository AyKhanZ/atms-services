namespace ATMS.Admin.Data.Entities;

public class Permission : DictionaryEntity<int>
{
    public string Module { get; set; }
    public List<RolePermission> RolePermissions { get; set; }
}
