using ATMS.Data.Constants;

namespace ATMS.Data.Criteria;

/// <summary>
/// The rule that binds everyone except a super administrator, who is bound by nothing:
/// <c>new ExceptSuperAdminCriteria&lt;WorkTask&gt;(roleId, new WorkTasksOfMyProjectsCriteria(userId))</c>.
/// </summary>
public sealed class ExceptSuperAdminCriteria<T>(Guid roleId, ACriteria<T> restriction) : ACriteria<T>
{
    public override IQueryable<T> Apply(IQueryable<T> query)
        => roleId == RoleIds.SuperAdmin ? query : restriction.Apply(query);
}
