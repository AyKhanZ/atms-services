using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATMS.Project.Data.Criteria.WorkTasks;

public sealed class WorkTasksByProjectCriteria(
    Guid projectId,
    Guid? workTicketId,
    Guid? parentWorkTaskId,
    bool rootTasksOnly,
    string? search = null) : ACriteria<WorkTask>
{
    public override IQueryable<WorkTask> Apply(IQueryable<WorkTask> query)
    {
        query = query.Where(task => task.WorkProjectId == projectId);

        if (workTicketId.HasValue)
        {
            query = query.Where(task => task.WorkTicketId == workTicketId.Value);
        }

        if (parentWorkTaskId.HasValue)
        {
            query = query.Where(task => task.ParentWorkTaskId == parentWorkTaskId.Value);
        }

        if (rootTasksOnly)
        {
            query = query.Where(task => task.ParentWorkTaskId == null);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            var pattern = $"%{term}%";
            query = query.Where(task =>
                EF.Functions.ILike(task.Code, pattern, "\\") ||
                EF.Functions.ILike(task.Title, pattern, "\\"));
        }

        return query;
    }
}
