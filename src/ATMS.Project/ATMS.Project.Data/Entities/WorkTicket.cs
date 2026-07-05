using ATMS.Project.Data.Entities.Dictionaries;

namespace ATMS.Project.Data.Entities;

public class WorkTicket : WorkItemEntity
{
    public Guid WorkGroupId { get; set; }
    
    public WorkGroup WorkGroup { get; set; }
    
    
    public int WorkTicketTypeId { get; set; }
    
    public WorkTicketType WorkTicketType { get; set; }
    
    
    public int WorkTicketStatusId { get; set; }
    
    public WorkTicketStatus WorkTicketStatus { get; set; }

    
    public ICollection<WorkTask> WorkTasks { get; set; } = [];

    public ICollection<Meeting> Meetings { get; set; } = [];
}
