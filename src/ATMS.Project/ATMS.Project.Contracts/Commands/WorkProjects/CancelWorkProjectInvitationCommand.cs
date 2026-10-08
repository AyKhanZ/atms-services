using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ParticipantInviteClient)]
public sealed class CancelWorkProjectInvitationCommand : IRequest, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    public Guid InvitationId { get; set; }
}
