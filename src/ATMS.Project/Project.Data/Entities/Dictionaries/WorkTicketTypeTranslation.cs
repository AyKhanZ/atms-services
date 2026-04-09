using ATMS.Data;
using ATMS.Data.Interfaces;

namespace Project.Data.Entities.Dictionaries;

public class WorkTicketTypeTranslation : BaseEntity<int>, ITranslation
{
    public int WorkTicketTypeId { get; set; }
    
    public string Language { get; set; }
    
    public string Name { get; set; }
    

    public WorkTicketType WorkTicketType { get; set; }
}
