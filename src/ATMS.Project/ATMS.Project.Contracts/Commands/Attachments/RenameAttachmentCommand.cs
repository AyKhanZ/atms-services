using System.Text.Json.Serialization;
using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Commands.Attachments;

[Access(PermissionEnum.ProjectEdit)]
[ProjectAccess(ProjectPermissionEnum.TaskEdit)]
public class RenameAttachmentCommand : IRequest, IProjectScopedRequest
{
    [JsonIgnore]
    public Guid ProjectId { get; set; }

    [JsonIgnore]
    public Guid AttachmentId { get; set; }

    public string? FileName { get; set; }
}
