using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ParticipantInviteClient)]
public sealed class InviteWorkProjectParticipantCommand : IRequest, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    public required string Email { get; set; }

    public required string Name { get; set; }

    public required string Surname { get; set; }
}
