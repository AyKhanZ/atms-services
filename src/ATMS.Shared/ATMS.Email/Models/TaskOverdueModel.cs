namespace ATMS.Email.Models;

public class TaskOverdueModel
{
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public required string TaskLabel { get; set; }
    public required string ProjectTitle { get; set; }
    public required string Deadline { get; set; }
    public required string Link { get; set; }
}
