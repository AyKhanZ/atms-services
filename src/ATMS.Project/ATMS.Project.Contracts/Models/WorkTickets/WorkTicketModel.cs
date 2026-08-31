using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.WorkTickets;

public class WorkTicketModel
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

    public DictionaryModel WorkTicketType { get; set; }

    public DictionaryModel WorkTicketStatus { get; set; }

    public DictionaryModel Priority { get; set; }

    public DateTime? Deadline { get; set; }

    public WorkTicketAssigneeModel? Assignee { get; set; }
}
