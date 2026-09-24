using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.WorkTasks;

// Deleting a project or a ticket does not mark its tasks deleted. The board list used to hide them
// only as a side effect of loading the project with the card, so the counts, which load nothing,
// still counted them for a super administrator. Said once here, it holds for both.
public sealed class WorkTasksOfLiveWorkCriteria : ACriteria<WorkTask>
{
    public override IQueryable<WorkTask> Apply(IQueryable<WorkTask> query)
        => query.Where(task => !task.WorkProject.IsDeleted && !task.WorkTicket.IsDeleted);
}
