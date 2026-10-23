namespace ATMS.Project.Contracts.Models.WorkTasks;

public sealed class WorkTaskDeadlineGroupModel
{
    public DateTime Deadline { get; set; }
    public int Total { get; set; }
    public WorkTaskModel[] WorkTasks { get; set; } = [];
}
