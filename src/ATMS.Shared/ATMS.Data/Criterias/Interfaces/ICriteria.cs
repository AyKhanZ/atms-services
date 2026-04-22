namespace ATMS.Data.Criterias.Interfaces;

public interface ICriteria<T>
{
    IQueryable<T> Apply(IQueryable<T> query);
}