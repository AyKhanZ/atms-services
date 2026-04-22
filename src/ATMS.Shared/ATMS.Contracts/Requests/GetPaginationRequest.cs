namespace ATMS.Contracts.Requests;

public abstract class GetPaginationRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}