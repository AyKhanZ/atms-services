namespace ATMS.Project.Data.Models.WorkTasks;

public sealed record WorkTaskCalendarResult(
    IReadOnlyList<WorkTaskDeadlineGroup> Groups,
    IReadOnlyDictionary<Guid, WorkTaskProgress> SubtaskProgress);
