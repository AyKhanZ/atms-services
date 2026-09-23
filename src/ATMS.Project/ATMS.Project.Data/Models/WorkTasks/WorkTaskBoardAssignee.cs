namespace ATMS.Project.Data.Models.WorkTasks;
public sealed record WorkTaskBoardAssignee(Guid UserId, string Name, string Surname, string? AvatarPath);
