namespace ATMS.Project.Data.Models.Attachments;

public sealed record AttachmentListItem(
    Guid Id,
    string FileName,
    string ContentType,
    long Size,
    DateTime CreatedAt,
    AttachmentAuthor CreatedBy,
    AttachmentOwnerTask WorkTask,
    AttachmentOwnerTask? ParentWorkTask);
