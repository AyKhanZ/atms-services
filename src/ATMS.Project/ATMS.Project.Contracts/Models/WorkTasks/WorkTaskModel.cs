using ATMS.Application.Models;
using ATMS.Project.Contracts.Models.WorkItems;

namespace ATMS.Project.Contracts.Models.WorkTasks;

public class WorkTaskModel
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid WorkProjectId { get; set; }
    public Guid WorkTicketId { get; set; }
    public string WorkTicketCode { get; set; }
    public string WorkTicketTitle { get; set; }
    public Guid MilestoneId { get; set; }
    public string MilestoneTitle { get; set; }
    public Guid GroupId { get; set; }
    public string GroupTitle { get; set; }
    public Guid? ParentWorkTaskId { get; set; }
    public string? ParentWorkTaskCode { get; set; }
    public string? ParentWorkTaskTitle { get; set; }
    public bool IsSubtask => ParentWorkTaskId.HasValue;
    public DictionaryModel Status { get; set; }
    public DictionaryModel Priority { get; set; }
    public DateTime? Deadline { get; set; }
    public WorkItemAssigneeModel? Assignee { get; set; }
    public int SubtaskCount { get; set; }
    public int DoneSubtaskCount { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public AuditUserModel? UpdatedBy { get; set; }
}
