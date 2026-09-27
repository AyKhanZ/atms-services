using ATMS.Application.Models;
using ATMS.Project.Contracts.Models.Dashboard;

namespace ATMS.Project.Contracts.Models.Comments;

public sealed class CommentReferenceModel
{
    public string Code { get; set; }

    public string Type { get; set; }

    public bool IsSubtask { get; set; }

    public string Title { get; set; }

    public DictionaryModel Status { get; set; }

    public DashboardRefModel Ref { get; set; }
}
