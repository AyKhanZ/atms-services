using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

[Access(PermissionEnum.ProjectEdit)]
[ProjectAccess(ProjectPermissionEnum.ParticipantDelete)]
public class DeleteWorkProjectParticipantCommand : IRequest, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    public Guid ParticipantId { get; set; }
}
