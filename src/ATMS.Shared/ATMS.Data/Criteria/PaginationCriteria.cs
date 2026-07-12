namespace ATMS.Data.Criteria;

public sealed class PaginationCriteria<T>(int page, int pageSize) : ACriteria<T>
{
    private const int MaxPageSize = 50;

    public int Page { get; } = page < 1 ? 1 : page;
    public int PageSize { get; } = pageSize > MaxPageSize ? MaxPageSize : pageSize < 1 ? 20 : pageSize;
    public int Skip => (Page - 1) * PageSize;

    // Пагинация применяется отдельно — ПОСЛЕ Count()
    // поэтому Apply здесь ничего не делает с фильтрами
    public override IQueryable<T> Apply(IQueryable<T> query)
        => query.Skip(Skip).Take(PageSize);
}