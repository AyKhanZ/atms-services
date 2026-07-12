using ATMS.Admin.Data.Entities;
using ATMS.Data.Criteria;

namespace ATMS.Admin.Data.Criteria.Users;

public class NotAdminCriteria : ACriteria<User>
{
    public override IQueryable<User> Apply(IQueryable<User> query)
    {
        return query.Where(user => !user.IsAdmin);
    }
}
