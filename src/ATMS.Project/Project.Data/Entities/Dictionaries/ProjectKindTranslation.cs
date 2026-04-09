using ATMS.Data;
using ATMS.Data.Interfaces;

namespace Project.Data.Entities.Dictionaries;

public class ProjectKindTranslation : BaseEntity<int>, ITranslation
{
    public int ProjectKindId { get; set; }
    
    public string Language { get; set; }
    
    public string Name { get; set; }
    

    public ProjectKind ProjectKind { get; set; }
}
