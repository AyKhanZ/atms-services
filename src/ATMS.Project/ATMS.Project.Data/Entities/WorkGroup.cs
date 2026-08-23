using ATMS.Data;
using ATMS.Data.Interfaces;
using ATMS.Project.Data.Entities.Dictionaries;

namespace ATMS.Project.Data.Entities;

public class WorkGroup : AuditableEntity, ISoftDeletable
{
    public string Title { get; set; }

    public Guid? ParentWorkGroupId { get; set; }  // null = Group, not null = Milestone
    
    public WorkGroup? ParentWorkGroup { get; set; }

    public ICollection<WorkGroup> Children { get; set; } = [];
    
    public ICollection<WorkTicket> WorkTickets { get; set; } = [];
    
    
    public int StatusId { get; set; }
    
    public WorkGroupStatus Status { get; set; }
    
    
    public Guid WorkProjectId { get; set; }
    
    public WorkProject WorkProject { get; set; }
    
    
    public bool IsDeleted { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public Guid? DeletedById { get; set; }
}
