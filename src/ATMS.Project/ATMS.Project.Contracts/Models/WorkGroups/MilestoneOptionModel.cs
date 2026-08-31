namespace ATMS.Project.Contracts.Models.WorkGroups;

public class MilestoneOptionModel
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public Guid GroupId { get; set; }

    public string GroupTitle { get; set; }
}
