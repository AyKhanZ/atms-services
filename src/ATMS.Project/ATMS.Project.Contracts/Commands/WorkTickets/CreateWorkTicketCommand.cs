using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkTickets;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.TicketCreate)]
public class CreateWorkTicketCommand : WorkTicketCommand, IRequest<Guid>, IProjectScopedRequest
{
}
