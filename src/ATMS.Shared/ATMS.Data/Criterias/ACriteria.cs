using ATMS.Data.Criterias.Interfaces;

namespace ATMS.Data.Criterias;

public abstract class ACriteria<T> : ICriteria<T>
{
    public abstract IQueryable<T> Apply(IQueryable<T> query);

    // Чейнинг: criteria.And(new EmailCriteria(...))
    public ACriteria<T> And(ACriteria<T> other)
        => new AndCriteria<T>(this, other);
}