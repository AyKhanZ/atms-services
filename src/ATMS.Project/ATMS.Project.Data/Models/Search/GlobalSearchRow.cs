using ATMS.Data.Enums;

namespace ATMS.Project.Data.Models.Search;

public class GlobalSearchRow
{
    public GlobalSearchItemType ItemType { get; set; }
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Title { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectCode { get; set; }
    public string ProjectTitle { get; set; }
    public int StatusId { get; set; }
    public string StatusCode { get; set; }
    public string StatusName { get; set; }
    public Guid? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public string? AssigneeSurname { get; set; }
    public string? AssigneeEmail { get; set; }
    public string? AssigneeAvatarPath { get; set; }
    public Guid? GroupId { get; set; }
    public string? GroupTitle { get; set; }
    public Guid? MilestoneId { get; set; }
    public string? MilestoneTitle { get; set; }
    public Guid? TicketId { get; set; }
    public string? TicketCode { get; set; }
    public string? TicketTitle { get; set; }
    public Guid? ParentTaskId { get; set; }
    public string? ParentTaskCode { get; set; }
    public string? ParentTaskTitle { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? OpenedAt { get; set; }
}
