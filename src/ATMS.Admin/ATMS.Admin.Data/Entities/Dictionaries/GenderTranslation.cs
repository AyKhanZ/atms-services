using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Admin.Data.Entities.Dictionaries;

public class GenderTranslation : BaseEntity<int>, ITranslation
{
    public int GenderId { get; set; }
    
    public string Language { get; set; }
    
    public string Name { get; set; }
    

    public Gender Gender { get; set; }
}
