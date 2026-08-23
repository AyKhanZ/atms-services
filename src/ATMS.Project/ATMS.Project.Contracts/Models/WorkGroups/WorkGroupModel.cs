using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.WorkGroups;

public class WorkGroupModel
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public Guid? ParentWorkGroupId { get; set; }

    public DictionaryModel Status { get; set; }

    public WorkGroupModel[] Milestones { get; set; } = [];

    public int TicketCount { get; set; }
}
