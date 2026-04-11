using ATMS.Data;

namespace ATMS.Project.Data.Entities.Dictionaries;

public class Permission : TranslatableDictionaryEntity
{
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
    
    public ICollection<PermissionTranslation> Translations { get; set; } = [];
}