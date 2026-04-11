using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Admin.Data.Entities.Dictionaries;

public class MaritalStatusTranslation : BaseEntity<int>, ITranslation
{
    public int MaritalStatusId { get; set; }
    
    public string Name { get; set; }
    
    public string Language { get; set; }

    
    public MaritalStatus MaritalStatus { get; set; }
}
