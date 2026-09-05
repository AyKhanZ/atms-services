using ATMS.Data.Criteria;
using ATMS.Data.Criteria.Users;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.Users;

/// <summary>
/// Users who work for the company: everyone who is neither a SuperAdmin nor a client-side user.
/// These are the only users eligible to be assigned work items.
/// </summary>
public sealed class EmployeeUsersCriteria : ACriteria<User>
{
    private readonly ACriteria<User> _criteria = new NotAdminCriteria<User>()
        .And(new NotClientUsersCriteria());

    public override IQueryable<User> Apply(IQueryable<User> query)
    {
        return _criteria.Apply(query);
    }
}
