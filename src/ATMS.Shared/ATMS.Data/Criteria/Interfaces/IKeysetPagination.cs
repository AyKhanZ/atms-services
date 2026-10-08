namespace ATMS.Data.Criteria.Interfaces;

public interface IKeysetPagination<T>
{
    IQueryable<T> Apply(IQueryable<T> query);

    KeysetPagedResult<T> ToResult(IReadOnlyList<T> items);
}
