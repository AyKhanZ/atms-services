using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkTickets;

[Access(PermissionEnum.ProjectEdit)]
[ProjectAccess(ProjectPermissionEnum.TicketDelete)]
public class DeleteWorkTicketCommand : IRequest, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    public Guid WorkTicketId { get; set; }
}
