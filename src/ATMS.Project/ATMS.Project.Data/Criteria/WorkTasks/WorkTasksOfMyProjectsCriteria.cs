using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.WorkTasks;

// deleted projects and participants drop out via the query filters
public sealed class WorkTasksOfMyProjectsCriteria(Guid userId) : ACriteria<WorkTask>
{
    public override IQueryable<WorkTask> Apply(IQueryable<WorkTask> query)
        => query.Where(task => task.WorkProject.WorkProjectParticipants
            .Any(participant => participant.UserId == userId));
}
