using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Project.Data.Entities.Dictionaries;

public class WorkItemPriorityTranslation : BaseEntity<int>, ITranslation
{
    public int WorkItemPriorityId { get; set; }
    
    public string Language { get; set; }
    
    public string Name { get; set; }
    

    public WorkItemPriority WorkItemPriority { get; set; }
}
