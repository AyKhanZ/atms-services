using ATMS.Data;
using ATMS.Data.Interfaces;

namespace Project.Data.Entities.Dictionaries;

public class WorkGroupStatusTranslation : BaseEntity<int>, ITranslation
{
    public int WorkGroupStatusId { get; set; }
    
    public string Language { get; set; }
    
    public string Name { get; set; }
    

    public WorkGroupStatus WorkGroupStatus { get; set; }
}

