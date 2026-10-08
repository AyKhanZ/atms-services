using ATMS.Application.Enums;
using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectAccessPolicyEnum.ParticipantInvite)]
public sealed class AddWorkProjectParticipantCommand : WorkProjectParticipantCommand, IRequest, IProjectRoleScopedRequest
{
    public Guid ProjectId { get; set; }
}
