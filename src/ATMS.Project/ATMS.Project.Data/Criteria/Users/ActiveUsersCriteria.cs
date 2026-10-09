using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.Users;

// inactive people stay on work they already have, they just can't be chosen again
public sealed class ActiveUsersCriteria : ACriteria<User>
{
    public override IQueryable<User> Apply(IQueryable<User> query)
    {
        return query.Where(user => user.IsActive);
    }
}
