using ATMS.Data;
using ATMS.Data.Interfaces;

namespace Project.Data.Entities.Dictionaries;

public class StateTranslation : BaseEntity<int>, ITranslation
{
    public string Name { get; set; }
    
    public string Language { get; set; }

    
    public State State { get; set; }
    
    public int StateId { get; set; }
}
