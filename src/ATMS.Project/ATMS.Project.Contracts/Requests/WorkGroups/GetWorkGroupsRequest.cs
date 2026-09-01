using ATMS.Project.Contracts.Models.WorkGroups;
using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Contracts.Requests.WorkGroups;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ProjectView)]
public class GetWorkGroupsRequest : IRequest<WorkGroupModel[]>, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }
}
