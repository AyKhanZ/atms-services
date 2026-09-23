using System.Text.Json.Serialization;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Commands.WorkTasks;
[Access(PermissionEnum.ProjectEdit)]
[ProjectAccess(ProjectPermissionEnum.TaskEdit)]
public class MoveWorkTaskCommand : IRequest, IProjectScopedRequest
{
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    [JsonIgnore]
    public Guid WorkTaskId { get; set; }

    public int StatusId { get; set; }

    public Guid? PreviousWorkTaskId { get; set; }

    public Guid? NextWorkTaskId { get; set; }

    public bool CompleteSubtasks { get; set; }
}
