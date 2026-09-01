using ATMS.Application.Security;
using ATMS.Contracts.Requests;
using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.WorkTickets;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Requests.WorkTickets;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ProjectView)]
public class GetWorkTicketsRequest : GetKeysetPaginationRequest,
    IRequest<KeysetPagedResult<WorkTicketModel>>,
    IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    /// <summary>Filter tickets by milestone. Omit to return tickets from all milestones in the project.</summary>
    public Guid? MilestoneId { get; init; }
}
