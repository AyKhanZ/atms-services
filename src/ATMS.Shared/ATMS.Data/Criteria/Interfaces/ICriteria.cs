namespace ATMS.Data.Criteria.Interfaces;

public interface ICriteria<T>
{
    IQueryable<T> Apply(IQueryable<T> query);
}