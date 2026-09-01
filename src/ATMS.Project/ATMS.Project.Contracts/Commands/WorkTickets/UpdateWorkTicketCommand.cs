using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkTickets;

[Access(PermissionEnum.ProjectEdit)]
[ProjectAccess(ProjectPermissionEnum.TicketEdit)]
public class UpdateWorkTicketCommand : WorkTicketCommand, IRequest, IProjectScopedRequest
{
    public Guid WorkTicketId { get; set; }

    public int WorkTicketStatusId { get; set; }
}
