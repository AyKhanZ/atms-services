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

    /// <summary>Ticket the task belongs to. Ignored for a subtask, which follows its parent.</summary>
    public Guid WorkTicketId { get; set; }

    /// <summary>Parent task for a subtask; empty turns the item into a top-level task.</summary>
    public Guid? ParentWorkTaskId { get; set; }
}
