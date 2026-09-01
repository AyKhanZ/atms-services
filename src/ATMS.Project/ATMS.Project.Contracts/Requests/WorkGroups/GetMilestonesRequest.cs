using ATMS.Application.Security;
using ATMS.Contracts.Requests;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.WorkGroups;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Requests.WorkGroups;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ProjectView)]
public class GetMilestonesRequest : GetKeysetPaginationRequest, IRequest<KeysetPagedResult<MilestoneOptionModel>>, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    /// <summary>Search by milestone or parent group name.</summary>
    public string? Search { get; init; }
}
