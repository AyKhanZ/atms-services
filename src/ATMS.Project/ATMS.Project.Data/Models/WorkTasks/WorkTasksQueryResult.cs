using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Models.WorkTasks;

public sealed record WorkTasksQueryResult(
    KeysetPagedResult<WorkTask> Page,
    IReadOnlyDictionary<Guid, WorkTaskProgress> SubtaskProgress);
