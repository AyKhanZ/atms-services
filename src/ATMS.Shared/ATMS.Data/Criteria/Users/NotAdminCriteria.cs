namespace ATMS.Data.Criteria.Users;

public sealed class NotAdminCriteria<TUser> : ACriteria<TUser>
    where TUser : UserAccountBase
{
    public override IQueryable<TUser> Apply(IQueryable<TUser> query)
    {
        return query.Where(user => !user.IsAdmin);
    }
}