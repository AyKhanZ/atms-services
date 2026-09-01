using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.WorkTickets;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Requests.WorkTickets;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ProjectView)]
public class GetWorkTicketRequest : IRequest<WorkTicketModel>, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    public Guid WorkTicketId { get; set; }
}
