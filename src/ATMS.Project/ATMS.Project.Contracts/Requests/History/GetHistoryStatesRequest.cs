using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.History;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Requests.History;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ProjectView)]
public class GetHistoryStatesRequest : IRequest<IReadOnlyCollection<HistoryStateModel>>, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    /// <summary>Statuses of this ticket. Without a ticket and a task, the statuses of the project.</summary>
    public Guid? WorkTicketId { get; init; }

    /// <summary>Statuses of this task or subtask.</summary>
    public Guid? WorkTaskId { get; init; }
}
