using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

[Access(PermissionEnum.ProjectDelete)]
[ProjectAccess(ProjectPermissionEnum.ProjectDelete)]
public class DeleteWorkProjectCommand : IRequest, IProjectScopedRequest
{
    public required Guid Id { get; set; }

    Guid IProjectScopedRequest.ProjectId => Id;
}
