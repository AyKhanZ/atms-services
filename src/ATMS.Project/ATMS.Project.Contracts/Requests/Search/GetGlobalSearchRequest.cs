using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Search;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Search;

[Access(PermissionEnum.ProjectView)]
public class GetGlobalSearchRequest : IRequest<GlobalSearchModel>
{
    /// <summary>An exact numeric code, or a title substring of at least three characters, up to 100 after trimming. Empty returns five recent items.</summary>
    public string? Q { get; set; }

    /// <summary>Maximum results per type, from 1 to 50. Defaults to 5; does not change the recent limit.</summary>
    public int Take { get; set; } = 5;
}
