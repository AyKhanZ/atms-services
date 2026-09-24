using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Commands.Attachments;

[Access(PermissionEnum.ProjectEdit)]
[ProjectAccess(ProjectPermissionEnum.TaskEdit)]
public class DeleteAttachmentCommand : IRequest, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    public Guid AttachmentId { get; set; }
}
