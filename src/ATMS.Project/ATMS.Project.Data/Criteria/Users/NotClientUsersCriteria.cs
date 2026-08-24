using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.Users;

public sealed class NotClientUsersCriteria : ACriteria<User>
{
    public override IQueryable<User> Apply(IQueryable<User> query)
    {
        return query.Where(user => user.UserType != (int)UserTypeEnum.Client);
    }
}
