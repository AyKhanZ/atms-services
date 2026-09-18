namespace ATMS.Project.Contracts.Models.Search;

public class GlobalSearchModel
{
    public GlobalSearchGroupModel Projects { get; set; }
    public GlobalSearchGroupModel Tickets { get; set; }
    public GlobalSearchGroupModel Tasks { get; set; }
    public GlobalSearchGroupModel Subtasks { get; set; }
    public GlobalSearchItemModel[] Recent { get; set; } = [];
}
