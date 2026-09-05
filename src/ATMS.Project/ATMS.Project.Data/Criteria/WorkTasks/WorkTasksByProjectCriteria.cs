using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.WorkTasks;

public sealed class WorkTasksByProjectCriteria(
    Guid projectId,
    Guid? workTicketId,
    Guid? parentWorkTaskId,
    bool rootTasksOnly) : ACriteria<WorkTask>
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

        return query;
    }
}
