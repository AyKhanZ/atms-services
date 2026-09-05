using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkTasks;

[Access(PermissionEnum.ProjectEdit)]
[ProjectAccess(ProjectPermissionEnum.TaskEdit)]
public class UpdateWorkTaskCommand : WorkTaskCommand, IRequest, IProjectScopedRequest
{
    public Guid WorkTaskId { get; set; }
    public int StatusId { get; set; }
}
