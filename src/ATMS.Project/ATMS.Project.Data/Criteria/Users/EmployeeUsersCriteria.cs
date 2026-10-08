using ATMS.Data.Criteria;
using ATMS.Data.Criteria.Users;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.Users;

// employees = not super admin and not client side, only they can be assigned work
public sealed class EmployeeUsersCriteria : ACriteria<User>
{
    private readonly ACriteria<User> _criteria = new NotAdminCriteria<User>()
        .And(new NotClientUsersCriteria());

    public override IQueryable<User> Apply(IQueryable<User> query)
    {
        return _criteria.Apply(query);
    }
}
