using ATMS.Application.Models;

namespace ATMS.Project.Contracts.Models.Attachments;

public class AttachmentAuthorModel : AuditUserModel
{
    public string? AvatarPath { get; set; }
}
