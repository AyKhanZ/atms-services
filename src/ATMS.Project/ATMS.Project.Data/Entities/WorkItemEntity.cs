using ATMS.Data;
using ATMS.Project.Data.Entities.Dictionaries;

namespace ATMS.Project.Data.Entities;

public abstract class WorkItemEntity : SoftDeletableAuditableEntity<User>
{
    public string Code { get; set; }
    
    public string Title { get; set; }
    
    public string? Description { get; set; }
    
    
    public int StatusId { get; set; }
    
    public WorkTaskStatus Status { get; set; }
    
    
    public int PriorityId { get; set; }
    
    public WorkItemPriority Priority { get; set; }
    
    
    public Guid? AssigneeId { get; set; }
    
    public WorkProjectParticipant? Assignee { get; set; }
    
    
    public DateTime? Deadline { get; set; }
    
    
    public Guid WorkProjectId { get; set; }
    
    public WorkProject WorkProject { get; set; }
}
