using ATMS.Application.Security;
using ATMS.Contracts.Requests;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.History;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Requests.History;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ProjectView)]
public class GetHistoryRequest : GetKeysetPaginationRequest, IRequest<KeysetPagedResult<HistoryEntryModel>>, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    /// <summary>History of this ticket. Without a ticket and a task, the project's own history with its groups and milestones.</summary>
    public Guid? WorkTicketId { get; init; }

    /// <summary>History of this task or subtask.</summary>
    public Guid? WorkTaskId { get; init; }
}
