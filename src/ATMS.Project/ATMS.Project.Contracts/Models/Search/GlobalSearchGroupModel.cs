namespace ATMS.Project.Contracts.Models.Search;

public sealed class GlobalSearchGroupModel
{
    public GlobalSearchItemModel[] Items { get; set; } = [];

    public bool HasMore { get; set; }
}
