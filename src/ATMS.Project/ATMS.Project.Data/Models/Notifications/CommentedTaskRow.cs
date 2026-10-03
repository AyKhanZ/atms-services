namespace ATMS.Project.Data.Models.Notifications;

public sealed record CommentedTaskRow(
    string ProjectTitle,
    string Code,
    string Title,
    bool IsSubtask,
    Guid? AssigneeUserId,
    Guid AuthorId,
    Guid[] CommenterIds);
