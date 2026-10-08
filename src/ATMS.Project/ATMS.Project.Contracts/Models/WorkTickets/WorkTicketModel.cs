using ATMS.Application.Models;
using ATMS.Project.Contracts.Models.Users;

namespace ATMS.Project.Contracts.Models.WorkTickets;

public sealed class WorkTicketModel
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

    public PersonModel? Assignee { get; set; }

    public int TotalTaskCount { get; set; }

    public int DoneTaskCount { get; set; }
}
