using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Admin.Data.Entities.Dictionaries;

public class UserTypeTranslation : BaseEntity<int>, ITranslation
{
    public int UserTypeId { get; set; }
    
    public string Language { get; set; }
    
    public string Name { get; set; }
    

    public UserType UserType { get; set; }
}
