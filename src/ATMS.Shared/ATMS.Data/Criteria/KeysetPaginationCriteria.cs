using System.Linq.Expressions;
using ATMS.Data.Enums;

namespace ATMS.Data.Criteria;

public sealed class KeysetPaginationCriteria<T>(string? cursor, int pageSize, SortDirectionEnum sortDirection)
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 50;

    public int PageSize { get; } = pageSize > MaxPageSize ? MaxPageSize : pageSize < 1 ? DefaultPageSize : pageSize;
    public SortDirectionEnum SortDirection { get; } = sortDirection;
    public KeysetCursor? Cursor { get; } = KeysetCursor.TryDecode(cursor, out var decoded) ? decoded : null;
    public int QuerySize => PageSize + 1;

    public IQueryable<T> Apply(
        IQueryable<T> query,
        Expression<Func<T, DateTime>> createdAtSelector,
        Expression<Func<T, Guid>> idSelector)
    {
        query = ApplyCursor(query, createdAtSelector, idSelector);

        return SortDirection == SortDirectionEnum.Asc
            ? query.OrderBy(createdAtSelector).ThenBy(idSelector).Take(QuerySize)
            : query.OrderByDescending(createdAtSelector).ThenByDescending(idSelector).Take(QuerySize);
    }

    public KeysetPagedResult<T> ToResult(IReadOnlyList<T> items, Func<T, DateTime> createdAtSelector, Func<T, Guid> idSelector)
    {
        var hasMore = items.Count > PageSize;
        var pageItems = items.Take(PageSize).ToArray();
        var last = pageItems.LastOrDefault();

        return new KeysetPagedResult<T>
        {
            Items = pageItems,
            HasMore = hasMore,
            PageSize = PageSize,
            NextCursor = hasMore && last is not null
                ? new KeysetCursor(createdAtSelector(last), idSelector(last), SortDirection).Encode()
                : null
        };
    }

    private IQueryable<T> ApplyCursor(
        IQueryable<T> query,
        Expression<Func<T, DateTime>> createdAtSelector,
        Expression<Func<T, Guid>> idSelector)
    {
        if (Cursor is null || Cursor.SortDirection != SortDirection)
        {
            return query;
        }

        var parameter = createdAtSelector.Parameters[0];
        var createdAt = createdAtSelector.Body;
        var id = ReplaceParameter(idSelector.Body, idSelector.Parameters[0], parameter);

        var cursorCreatedAt = Expression.Constant(Cursor.CreatedAt, typeof(DateTime));
        var cursorId = Expression.Constant(Cursor.Id, typeof(Guid));

        var dateComparison = SortDirection == SortDirectionEnum.Asc
            ? Expression.GreaterThan(createdAt, cursorCreatedAt)
            : Expression.LessThan(createdAt, cursorCreatedAt);

        var idComparison = SortDirection == SortDirectionEnum.Asc
            ? Expression.GreaterThan(id, cursorId)
            : Expression.LessThan(id, cursorId);

        var predicate = Expression.Lambda<Func<T, bool>>(
            Expression.OrElse(
                dateComparison,
                Expression.AndAlso(Expression.Equal(createdAt, cursorCreatedAt), idComparison)),
            parameter);

        return query.Where(predicate);
    }

    private static Expression ReplaceParameter(Expression expression, ParameterExpression source, ParameterExpression target)
        => new ParameterReplaceVisitor(source, target).Visit(expression)!;

    private sealed class ParameterReplaceVisitor(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == source ? target : base.VisitParameter(node);
    }
}
