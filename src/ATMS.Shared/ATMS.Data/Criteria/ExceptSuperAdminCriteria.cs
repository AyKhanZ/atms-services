using ATMS.Data.Constants;

namespace ATMS.Data.Criteria;

// e.g. new ExceptSuperAdminCriteria<WorkTask>(roleId, new WorkTasksOfMyProjectsCriteria(userId))
public sealed class ExceptSuperAdminCriteria<T>(Guid roleId, ACriteria<T> restriction) : ACriteria<T>
{
    public override IQueryable<T> Apply(IQueryable<T> query)
        => roleId == RoleIds.SuperAdmin ? query : restriction.Apply(query);
}
