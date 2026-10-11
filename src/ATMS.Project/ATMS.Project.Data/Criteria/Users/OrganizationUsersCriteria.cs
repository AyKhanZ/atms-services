using ATMS.Data.Criteria;
using ATMS.Project.Data.Entities;

namespace ATMS.Project.Data.Criteria.Users;

public sealed class OrganizationUsersCriteria(Guid organizationId) : ACriteria<User>
{
    public override IQueryable<User> Apply(IQueryable<User> query)
    {
        return query.Where(user => user.OrganizationId == organizationId);
    }
}
