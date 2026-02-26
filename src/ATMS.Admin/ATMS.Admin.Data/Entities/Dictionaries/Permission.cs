namespace ATMS.Admin.Data.Entities.Dictionaries;

public class Permission : DictionaryEntity
{
    public string Module { get; set; }
    public List<RolePermission> RolePermissions { get; set; }
}
