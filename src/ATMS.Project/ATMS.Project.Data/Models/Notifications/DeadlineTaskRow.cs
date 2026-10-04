namespace ATMS.Project.Data.Models.Notifications;

public sealed record DeadlineTaskRow(
    Guid Id,
    Guid ProjectId,
    string ProjectTitle,
    string Code,
    string Title,
    bool IsSubtask,
    DateTime Deadline,
    Guid? AssigneeUserId,
    Guid[] ManagerUserIds);
