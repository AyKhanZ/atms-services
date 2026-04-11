using ATMS.Data;

namespace ATMS.Admin.Data.Entities.Dictionaries;

public class Permission : TranslatableDictionaryEntity
{
    public string Module { get; set; }
    public List<RolePermission> RolePermissions { get; set; } = [];
    public ICollection<PermissionTranslation> Translations { get; set; } = [];
}
