using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.WorkTasks;

/// <summary>
/// Work from the projects the user takes part in. Deleted projects and participants drop out on their
/// own: the query filters of the context reach a navigation as well.
/// </summary>
public sealed class WorkTasksOfMyProjectsCriteria(Guid userId) : ACriteria<WorkTask>
{
    public override IQueryable<WorkTask> Apply(IQueryable<WorkTask> query)
        => query.Where(task => task.WorkProject.WorkProjectParticipants
            .Any(participant => participant.UserId == userId));
}
