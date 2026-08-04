using ATMS.Data.Constants;
using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.WorkProjects;

public class AccessibleWorkProjectsCriteria(Guid userId, Guid roleId) : ACriteria<WorkProject>
{
    public override IQueryable<WorkProject> Apply(IQueryable<WorkProject> query)
    {
        if (roleId == RoleIds.SuperAdmin)
        {
            return query;
        }

        return query.Where(project => project.WorkProjectParticipants.Any(participant => participant.UserId == userId));
    }
}
