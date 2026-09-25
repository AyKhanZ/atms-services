namespace ATMS.Project.Data.Models.Dashboard;

public sealed record DashboardDeadlineRow(
    Guid Id,
    Guid WorkProjectId,
    Guid WorkTicketId,
    string Code,
    string Title,
    bool IsSubtask,
    DateTime Deadline,
    int PriorityId,
    Guid? AssigneeUserId,
    string? AssigneeName,
    string? AssigneeSurname,
    string? AssigneeAvatarPath);
