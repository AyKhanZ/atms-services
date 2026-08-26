using ATMS.Project.Contracts.Models.WorkProjects;
using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Contracts.Requests.WorkProjects;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ProjectView)]
public class GetWorkProjectRequest : IRequest<WorkProjectModel>, IProjectScopedRequest
{
    public Guid Id { get; set; }

    Guid IProjectScopedRequest.ProjectId => Id;
}
