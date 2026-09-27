namespace ATMS.Project.Data.Models.Comments;

public sealed record CommentWorkItemReferenceRow(
    string Code,
    bool IsTicket,
    bool IsSubtask,
    string Title,
    int StatusId,
    Guid ProjectId,
    Guid? WorkTicketId,
    Guid? WorkTaskId);
