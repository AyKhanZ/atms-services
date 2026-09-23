namespace ATMS.Data.Criteria.Interfaces;

/// <summary>One page of a keyset-ordered list, whatever column it is ordered by.</summary>
public interface IKeysetPagination<T>
{
    IQueryable<T> Apply(IQueryable<T> query);

    KeysetPagedResult<T> ToResult(IReadOnlyList<T> items);
}
