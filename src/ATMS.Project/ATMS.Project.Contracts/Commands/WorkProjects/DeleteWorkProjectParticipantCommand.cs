using ATMS.Application.Enums;
using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectAccessPolicyEnum.ParticipantDelete)]
public sealed class DeleteWorkProjectParticipantCommand : IRequest, IProjectParticipantScopedRequest
{
    public Guid ProjectId { get; set; }

    public Guid ParticipantId { get; set; }
}
