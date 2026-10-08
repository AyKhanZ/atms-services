using ATMS.Data.Criteria.Interfaces;

namespace ATMS.Data.Criteria;

public abstract class ACriteria<T> : ICriteria<T>
{
    public abstract IQueryable<T> Apply(IQueryable<T> query);

    public ACriteria<T> And(ACriteria<T> other)
        => new AndCriteria<T>(this, other);
}