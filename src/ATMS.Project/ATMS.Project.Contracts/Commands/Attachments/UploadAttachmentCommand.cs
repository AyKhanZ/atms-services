using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Attachments;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ATMS.Project.Contracts.Commands.Attachments;

[Access(PermissionEnum.ProjectEdit)]
[ProjectAccess(ProjectPermissionEnum.TaskEdit)]
public class UploadAttachmentCommand : IRequest<AttachmentModel>, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    public Guid WorkTaskId { get; set; }

    public IFormFile? File { get; set; }
}
