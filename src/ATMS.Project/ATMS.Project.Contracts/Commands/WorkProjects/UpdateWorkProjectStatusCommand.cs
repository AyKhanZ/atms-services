using System.Text.Json.Serialization;
using MediatR;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;

namespace ATMS.Project.Contracts.Commands.WorkProjects;

[Access(PermissionEnum.ProjectEdit)]
[ProjectAccess(ProjectPermissionEnum.ProjectEdit)]
public class UpdateWorkProjectStatusCommand : IRequest, IProjectScopedRequest
{
    [JsonIgnore]
    public Guid Id { get; set; }

    Guid IProjectScopedRequest.ProjectId => Id;

    public required int ProjectStatusId { get; set; }
}
