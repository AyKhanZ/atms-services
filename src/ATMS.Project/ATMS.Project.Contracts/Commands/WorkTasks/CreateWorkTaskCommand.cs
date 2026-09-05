using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkTasks;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.TaskCreate)]
public class CreateWorkTaskCommand : WorkTaskCommand, IRequest<Guid>, IProjectScopedRequest
{
    public Guid WorkTicketId { get; set; }
    public Guid? ParentWorkTaskId { get; set; }
}
