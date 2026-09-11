using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.WorkTasks;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Requests.WorkTasks;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ProjectView)]
public class GetWorkTaskRequest : IRequest<WorkTaskModel>, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }
    public Guid WorkTaskId { get; set; }
}
