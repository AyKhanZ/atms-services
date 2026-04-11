using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Project.Data.Entities.Dictionaries;

public class ProjectTypeTranslation : BaseEntity<int>, ITranslation
{
    public string Name { get; set; }
    
    public string Language { get; set; }
    

    public int ProjectTypeId { get; set; }
    
    public ProjectType ProjectType { get; set; }
}
