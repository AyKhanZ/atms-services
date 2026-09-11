namespace ATMS.Project.Contracts.Models.WorkItems;

public class WorkItemAssigneeModel
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Surname { get; set; }

    public string? AvatarPath { get; set; }
}
