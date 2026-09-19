namespace ATMS.Project.Contracts.Models.Search;

public class GlobalSearchGroupModel
{
    public GlobalSearchItemModel[] Items { get; set; } = [];

    public bool HasMore { get; set; }
}
