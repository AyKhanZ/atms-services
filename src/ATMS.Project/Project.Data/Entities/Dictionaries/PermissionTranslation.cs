using ATMS.Data;
using ATMS.Data.Interfaces;

namespace Project.Data.Entities.Dictionaries;

public class PermissionTranslation: BaseEntity<int>, ITranslation
{
    public int PermissionId { get; set; }
    
    public string Language { get; set; }
    
    public string Name { get; set; }
    

    public Permission Permission { get; set; }
}
