using ATMS.Application.Models;
using ATMS.Project.Contracts.Models.Users;

namespace ATMS.Project.Contracts.Models.WorkTasks;

public sealed class WorkTaskModel
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public Guid WorkProjectId { get; set; }
    public Guid MilestoneId { get; set; }
    public string MilestoneTitle { get; set; }
    public Guid GroupId { get; set; }
    public string GroupTitle { get; set; }
    public DictionaryModel<Guid>? WorkProject { get; set; }
    public DictionaryModel<Guid> WorkTicket { get; set; }
    public DictionaryModel<Guid>? ParentWorkTask { get; set; }
    public bool IsSubtask => ParentWorkTask is not null;
    public DictionaryModel Status { get; set; }
    public DictionaryModel Priority { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? DoneAt { get; set; }
    public PersonModel? Assignee { get; set; }
    public int SubtaskCount { get; set; }
    public int DoneSubtaskCount { get; set; }
    public int CommentsCount { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public AuditUserModel? UpdatedBy { get; set; }
}
