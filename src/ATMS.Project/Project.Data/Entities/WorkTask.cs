namespace Project.Data.Entities;

public class WorkTask : WorkItemEntity
{
    public Guid? ParentWorkTaskId { get; set; }
    
    public Task? ParentWorkTask { get; set; }
    
    public uint Level { get; set; }
    
    
    public ICollection<WorkTask> Children { get; set; } = [];
    
    
    public Guid WorkTicketId { get; set; }
    
    public WorkTicket WorkTicket { get; set; }
}
