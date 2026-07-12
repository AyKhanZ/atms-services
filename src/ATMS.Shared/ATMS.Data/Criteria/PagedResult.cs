namespace ATMS.Data.Criteria;

public sealed class PagedResult<T>
{
    public T[] Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
    
    public PagedResult<TResult> Map<TResult>(Func<T, TResult> map)
        => new()
        {
            Items      = Items.Select(map).ToArray(),
            TotalCount = TotalCount,
            Page       = Page,
            PageSize   = PageSize
        };
}