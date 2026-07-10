using ATMS.Data.Enums;

namespace ATMS.Contracts.Requests;

public abstract class GetPaginationRequest
{
    /// <summary>Page number (default: 1)</summary>
    public int Page { get; init; } = 1;
    
    /// <summary>Page size (default: 20, max: 50)</summary>
    public int PageSize { get; init; } = 20;
    
    /// <summary>Field to sort by: title, createdat</summary>
    public string? SortBy { get; init; }
    
    /// <summary>Sort direction: Asc(1) or Desc(2) (default: Asc)</summary>
    public SortDirectionEnum SortDirection { get; init; } = SortDirectionEnum.Asc;
}
