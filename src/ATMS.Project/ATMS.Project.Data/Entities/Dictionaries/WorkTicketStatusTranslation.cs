using ATMS.Data;
using ATMS.Data.Interfaces;

namespace ATMS.Project.Data.Entities.Dictionaries;

public class WorkTicketStatusTranslation : BaseEntity<int>, ITranslation
{
    public int WorkTicketStatusId { get; set; }
    
    public string Language { get; set; }
    
    public string Name { get; set; }
    

    public WorkTicketStatus WorkTicketStatus { get; set; }
}
