using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Attachments;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Attachments;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ProjectView)]
public class GetAttachmentsTreeRequest : IRequest<AttachmentTreeModel>, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }
}
