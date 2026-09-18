using ATMS.Application.Models;
using ATMS.Project.Contracts.Models.Users;

namespace ATMS.Project.Contracts.Models.Search;

public class GlobalSearchItemModel
{
    public int ItemType { get; set; }
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Title { get; set; }

    public DictionaryModel<Guid> Project { get; set; }
    public DictionaryModel Status { get; set; }
    public UserModel Assignee { get; set; }
    public GlobalSearchLocationModel Group { get; set; }
    public GlobalSearchLocationModel Milestone { get; set; }
    public DictionaryModel<Guid> Ticket { get; set; }
    public DictionaryModel<Guid> ParentTask { get; set; }

    public DateTime? OpenedAt { get; set; }
}
