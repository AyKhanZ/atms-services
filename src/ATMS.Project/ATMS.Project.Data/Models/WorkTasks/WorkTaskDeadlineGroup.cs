using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Models.WorkTasks;

public sealed record WorkTaskDeadlineGroup(DateTime Deadline, int Total, IReadOnlyList<WorkTask> WorkTasks);
