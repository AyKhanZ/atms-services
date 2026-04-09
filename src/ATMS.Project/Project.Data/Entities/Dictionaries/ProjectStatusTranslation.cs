using ATMS.Data;
using ATMS.Data.Interfaces;

namespace Project.Data.Entities.Dictionaries;

public class ProjectStatusTranslation : BaseEntity<int>, ITranslation
{
    public int ProjectStatusId { get; set; }
    
    public string Language { get; set; }
    
    public string Name { get; set; }
    

    public ProjectStatus ProjectStatus { get; set; }
}
