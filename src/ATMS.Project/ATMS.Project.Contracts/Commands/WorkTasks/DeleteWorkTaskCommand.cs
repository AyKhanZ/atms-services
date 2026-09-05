using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkTasks;

[Access(PermissionEnum.ProjectEdit)]
[ProjectAccess(ProjectPermissionEnum.TaskDelete)]
public class DeleteWorkTaskCommand : IRequest, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }
    public Guid WorkTaskId { get; set; }
}
