using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Users;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Users;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ParticipantInviteClient)]
public sealed class GetProjectClientCandidatesRequest : IRequest<UserModel[]>, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }
}
