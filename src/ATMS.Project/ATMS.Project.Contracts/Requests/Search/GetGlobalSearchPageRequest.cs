using ATMS.Application.Security;
using ATMS.Contracts.Requests;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Search;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Search;

[Access(PermissionEnum.ProjectView)]
public class GetGlobalSearchPageRequest : GetKeysetPaginationRequest,
    IRequest<KeysetPagedResult<GlobalSearchItemModel>>
{
    /// <summary>Which of the four kinds to list: Project, Ticket, Task or Subtask.</summary>
    public GlobalSearchItemType ItemType { get; set; }

    /// <summary>An exact numeric code, or a title substring of at least three characters, up to 100 after trimming.</summary>
    public string? Q { get; set; }
}
