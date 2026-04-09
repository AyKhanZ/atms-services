using ATMS.Data;
using Project.Data.Entities.Dictionaries;

namespace Project.Data.Entities;

public class WorkGroup : AuditableEntity
{
    public string Title { get; set; }
    
    
    public Guid? ParentWorkGroupId { get; set; }  // null = Group, not null = Milestone
    
    public WorkGroup? ParentWorkGroup { get; set; }
    
    
    public uint Level { get; set; }
    
    
    public ICollection<WorkGroup> Children { get; set; } = [];
    
    public ICollection<WorkTicket> WorkTickets { get; set; } = [];
    
    
    public uint StatusId { get; set; }
    
    public WorkGroupStatus Status { get; set; }
    
    
    public Guid WorkProjectId { get; set; }
    
    public WorkProject WorkProject { get; set; }
}
