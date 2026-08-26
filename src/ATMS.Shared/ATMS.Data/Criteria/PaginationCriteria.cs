using ATMS.Application.Exceptions.Resources;

namespace ATMS.Data.Criteria;

public sealed class PaginationCriteria<T> : ACriteria<T>
{
    private const int MaxPageSize = 50;

    public PaginationCriteria(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new CriteriaException(nameof(page), ValidationMessages.PageMustBePositive);
        }

        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            throw new CriteriaException(nameof(pageSize), ValidationMessages.PageSizeOutOfRange);
        }

        Page = page;
        PageSize = pageSize;
    }

    public int Page { get; }
    public int PageSize { get; }
    public int Skip => (Page - 1) * PageSize;

    // Пагинация применяется отдельно — ПОСЛЕ Count()
    // поэтому Apply здесь ничего не делает с фильтрами
    public override IQueryable<T> Apply(IQueryable<T> query)
        => query.Skip(Skip).Take(PageSize);
}
