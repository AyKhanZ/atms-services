using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.WorkTasks;

// deleting a project or a ticket doesn't mark its tasks deleted, so they are hidden here
public sealed class WorkTasksOfLiveWorkCriteria : ACriteria<WorkTask>
{
    public override IQueryable<WorkTask> Apply(IQueryable<WorkTask> query)
        => query.Where(task => !task.WorkProject.IsDeleted && !task.WorkTicket.IsDeleted);
}
