using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Requests.WorkProjects;

[Access(PermissionEnum.ProjectView)]
public class GetMyProjectPermissionsRequest : IRequest<string[]>, IProjectScopedRequest
{
    public Guid ProjectId { get; init; }
}
