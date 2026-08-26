using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

[Access(PermissionEnum.ProjectEdit)]
[ProjectAccess(ProjectPermissionEnum.ProjectEdit)]
public class UpdateWorkProjectParticipantCommand : IRequest, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    public Guid ParticipantId { get; set; }

    public Guid RoleId { get; set; }
}
