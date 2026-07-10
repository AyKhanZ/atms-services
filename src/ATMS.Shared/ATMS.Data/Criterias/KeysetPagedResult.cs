namespace ATMS.Data.Criterias;

public sealed class KeysetPagedResult<T>
{
    public T[] Items { get; init; } = [];
    public string? NextCursor { get; init; }
    public bool HasMore { get; init; }
    public int PageSize { get; init; }

    public KeysetPagedResult<TResult> Map<TResult>(Func<T, TResult> map)
        => new()
        {
            Items = Items.Select(map).ToArray(),
            NextCursor = NextCursor,
            HasMore = HasMore,
            PageSize = PageSize
        };
}
