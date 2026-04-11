using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Admin.Data.Entities.Dictionaries;

public class UserStatusTranslation : BaseEntity<int>, ITranslation
{
    public int UserStatusId { get; set; }
    
    public string Language { get; set; }
    
    public string Name { get; set; }
    

    public UserStatus UserStatus { get; set; }
}
