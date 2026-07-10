using ATMS.Admin.Data.Entities;
using ATMS.Data.Criterias;

namespace ATMS.Admin.Data.Criterias.Users;

public class NotAdminCriteria : ACriteria<User>
{
    public override IQueryable<User> Apply(IQueryable<User> query)
    {
        return query.Where(user => !user.IsAdmin);
    }
}
