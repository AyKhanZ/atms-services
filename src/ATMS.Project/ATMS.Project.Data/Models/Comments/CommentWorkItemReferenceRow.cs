namespace ATMS.Project.Data.Models.Comments;

public sealed record CommentWorkItemReferenceRow(
    string Code,
    CommentReferenceKind Kind,
    bool IsSubtask,
    string Title,
    int StatusId,
    Guid ProjectId,
    Guid? WorkTicketId,
    Guid? WorkTaskId);
