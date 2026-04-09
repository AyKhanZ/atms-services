using ATMS.Data;
using Project.Data.Entities.Dictionaries;
using TaskStatus = System.Threading.Tasks.TaskStatus;

namespace Project.Data.Entities;

public class WorkItemEntity : AuditableEntity
{
    public string Title { get; set; }
    
    public string? Description { get; set; }
    
    
    public uint StatusId { get; set; }
    
    public TaskStatus Status { get; set; }
    
    
    public uint PriorityId { get; set; }
    
    public WorkItemPriority Priority { get; set; }
    
    
    public Guid? AssigneeId { get; set; }
    
    public WorkProjectParticipant? Assignee { get; set; }
    
    
    public DateTime? Deadline { get; set; }
    
    
    public Guid WorkProjectId { get; set; }
    
    public WorkProject WorkProject { get; set; }
}
