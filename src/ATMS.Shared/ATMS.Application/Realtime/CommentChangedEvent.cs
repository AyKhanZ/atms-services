namespace ATMS.Application.Realtime;

public sealed record CommentChangedEvent(
    Guid WorkTaskId,
    Guid CommentId,
    string Action);
