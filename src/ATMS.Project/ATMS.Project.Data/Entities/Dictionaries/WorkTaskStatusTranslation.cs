using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Project.Data.Entities.Dictionaries;

public class WorkTaskStatusTranslation : BaseEntity<int>, ITranslation
{
    public int WorkTaskStatusId { get; set; }
    
    public string Language { get; set; }
    
    public string Name { get; set; }
    

    public WorkTaskStatus WorkTaskStatus { get; set; }
}
