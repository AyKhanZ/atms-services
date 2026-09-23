using System.Text.Json.Serialization;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkTasks;
[Access(PermissionEnum.ProjectEdit)]
[ProjectAccess(ProjectPermissionEnum.TaskEdit)]
public class UpdateWorkTaskDeadlineCommand : IRequest, IProjectScopedRequest
{
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    [JsonIgnore]
    public Guid WorkTaskId { get; set; }

    public DateTime? Deadline { get; set; }
}
