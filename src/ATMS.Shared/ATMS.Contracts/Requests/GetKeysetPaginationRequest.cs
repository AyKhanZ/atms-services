using ATMS.Data.Enums;

namespace ATMS.Contracts.Requests;

public abstract class GetKeysetPaginationRequest
{
    /// <summary>Opaque continuation token from the previous response.</summary>
    public string? Cursor { get; init; }

    /// <summary>Page size (default: 20, max: 50).</summary>
    public int PageSize { get; init; } = 20;

    /// <summary>Sort direction: Asc(1) or Desc(2) (default: Desc).</summary>
    public SortDirectionEnum SortDirection { get; init; } = SortDirectionEnum.Desc;
}
