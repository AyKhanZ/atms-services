using ATMS.Application.Security;
using ATMS.Contracts.Requests;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.WorkTasks;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Requests.WorkTasks;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ProjectView)]
public class GetWorkTasksRequest : GetKeysetPaginationRequest,
    IRequest<KeysetPagedResult<WorkTaskModel>>,
    IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    /// <summary>Filter tasks by their ticket. Omit to return tasks from all tickets in the project.</summary>
    public Guid? WorkTicketId { get; init; }

    /// <summary>Return only direct children of this task. Cannot be combined with rootTasksOnly.</summary>
    public Guid? ParentWorkTaskId { get; init; }

    /// <summary>When true, return only top-level tasks. Cannot be combined with parentWorkTaskId.</summary>
    public bool RootTasksOnly { get; init; }
}
