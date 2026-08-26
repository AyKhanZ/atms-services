using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

[Access(PermissionEnum.ProjectEdit)]
[ProjectAccess(ProjectPermissionEnum.ProjectEdit)]
public class AddWorkProjectParticipantCommand : WorkProjectParticipantCommand, IRequest, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }
}
