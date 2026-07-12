namespace ATMS.Data.Criteria;

internal sealed class AndCriteria<T>(
    ACriteria<T> left,
    ACriteria<T> right) : ACriteria<T>
{
    public override IQueryable<T> Apply(IQueryable<T> query)
        => right.Apply(left.Apply(query));
}