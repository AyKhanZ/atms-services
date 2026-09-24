using ATMS.Application.Security;
using ATMS.Data.Enums;
using ATMS.Project.Contracts.Models.Attachments;
using ATMS.Project.Contracts.Requests.Security;
using MediatR;

namespace ATMS.Project.Contracts.Requests.Attachments;

[Access(PermissionEnum.ProjectView)]
[ProjectAccess(ProjectPermissionEnum.ProjectView)]
public class GetAttachmentsRequest : IRequest<AttachmentListModel>, IProjectScopedRequest
{
    public Guid ProjectId { get; set; }

    /// <summary>Files uploaded to this task or subtask itself.</summary>
    public Guid? WorkTaskId { get; init; }

    /// <summary>Files uploaded to the subtasks of this task, without the task's own files.</summary>
    public Guid? ParentWorkTaskId { get; init; }

    /// <summary>Files of every task and subtask in this ticket.</summary>
    public Guid? WorkTicketId { get; init; }
}
