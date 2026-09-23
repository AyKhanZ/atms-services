using System.Linq.Expressions;
using ATMS.Application.Exceptions.Resources;
using ATMS.Data.Criteria.Interfaces;
using ATMS.Data.Enums;

namespace ATMS.Data.Criteria;

/// <summary>
/// One page of a list ordered by a key and the id behind it. The key is whatever column the list is
/// ordered by: a creation date, a deadline, a priority, a manual rank.
/// </summary>
public class KeysetPaginationCriteria<T, TKey> : IKeysetPagination<T>
{
    private const int MaxPageSize = 50;

    private readonly Expression<Func<T, TKey>> _keySelector;
    private readonly Expression<Func<T, Guid>> _idSelector;
    private readonly Lazy<Func<T, TKey>> _key;
    private readonly Lazy<Func<T, Guid>> _id;
    private readonly bool _emptyKeysLast;

    public KeysetPaginationCriteria(
        string? cursor,
        int pageSize,
        SortDirectionEnum sortDirection,
        Expression<Func<T, TKey>> keySelector,
        Expression<Func<T, Guid>> idSelector,
        bool emptyKeysLast = false)
    {
        PageSize = ValidatePageSize(pageSize);
        SortDirection = ValidateSortDirection(sortDirection);
        Cursor = DecodeCursor(cursor, SortDirection);
        _keySelector = keySelector;
        _idSelector = idSelector;
        _key = new Lazy<Func<T, TKey>>(keySelector.Compile);
        _id = new Lazy<Func<T, Guid>>(idSelector.Compile);
        _emptyKeysLast = emptyKeysLast;
    }

    public int PageSize { get; }
    public SortDirectionEnum SortDirection { get; }
    public KeysetCursor? Cursor { get; }
    public int QuerySize => PageSize + 1;

    public IQueryable<T> Apply(IQueryable<T> query)
    {
        var ascending = SortDirection == SortDirectionEnum.Asc;
        query = ApplyCursor(query);

        // Rows without a key go last whichever way the rest is ordered; otherwise a page of empty
        // deadlines would open the list.
        var ordered = _emptyKeysLast
            ? query.OrderBy(EmptyKeySelector())
            : null;

        var byKey = ordered is null
            ? (ascending ? query.OrderBy(_keySelector) : query.OrderByDescending(_keySelector))
            : (ascending ? ordered.ThenBy(_keySelector) : ordered.ThenByDescending(_keySelector));

        return (ascending ? byKey.ThenBy(_idSelector) : byKey.ThenByDescending(_idSelector))
            .Take(QuerySize);
    }

    public KeysetPagedResult<T> ToResult(IReadOnlyList<T> items)
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
                ? KeysetCursor.For(_key.Value(last), _id.Value(last), SortDirection).Encode()
                : null
        };
    }

    private Expression<Func<T, bool>> EmptyKeySelector()
    {
        var parameter = _keySelector.Parameters[0];

        return Expression.Lambda<Func<T, bool>>(
            Expression.Equal(_keySelector.Body, Expression.Constant(null, typeof(TKey))),
            parameter);
    }

    private IQueryable<T> ApplyCursor(IQueryable<T> query)
    {
        if (Cursor is null || Cursor.SortDirection != SortDirection)
        {
            return query;
        }

        var parameter = _keySelector.Parameters[0];
        var key = _keySelector.Body;
        var id = ReplaceParameter(_idSelector.Body, _idSelector.Parameters[0], parameter);
        var cursorId = Expression.Constant(Cursor.Id, typeof(Guid));
        var ascending = SortDirection == SortDirectionEnum.Asc;
        var idComparison = ascending
            ? Expression.GreaterThan(id, cursorId)
            : Expression.LessThan(id, cursorId);

        // The page ended among the rows without a key: only they are left.
        if (_emptyKeysLast && Cursor.Key.Length == 0)
        {
            var noKey = Expression.Equal(key, Expression.Constant(null, typeof(TKey)));

            return query.Where(Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(noKey, idComparison),
                parameter));
        }

        var cursorKey = Expression.Constant(Cursor.KeyAs<TKey>(), typeof(TKey));
        var keyComparison = Compare(key, cursorKey, ascending);

        Expression predicate = Expression.OrElse(
            keyComparison,
            Expression.AndAlso(Expression.Equal(key, cursorKey), idComparison));

        if (_emptyKeysLast)
        {
            predicate = Expression.OrElse(
                predicate,
                Expression.Equal(key, Expression.Constant(null, typeof(TKey))));
        }

        return query.Where(Expression.Lambda<Func<T, bool>>(predicate, parameter));
    }

    /// <summary>Text is compared by the database's own order; everything else by its operators.</summary>
    private static Expression Compare(Expression key, Expression cursorKey, bool ascending)
    {
        if (typeof(TKey) != typeof(string))
        {
            return ascending
                ? Expression.GreaterThan(key, cursorKey, liftToNull: false, method: null)
                : Expression.LessThan(key, cursorKey, liftToNull: false, method: null);
        }

        var compare = Expression.Call(
            typeof(string).GetMethod(nameof(string.Compare), [typeof(string), typeof(string)])!,
            key,
            cursorKey);
        var zero = Expression.Constant(0);

        return ascending ? Expression.GreaterThan(compare, zero) : Expression.LessThan(compare, zero);
    }

    private static Expression ReplaceParameter(Expression expression, ParameterExpression source, ParameterExpression target)
        => new ParameterReplaceVisitor(source, target).Visit(expression)!;

    private static int ValidatePageSize(int value)
    {
        if (value < 1 || value > MaxPageSize)
        {
            throw new CriteriaException("pageSize", ValidationMessages.PageSizeOutOfRange);
        }

        return value;
    }

    private static SortDirectionEnum ValidateSortDirection(SortDirectionEnum value)
    {
        if (!Enum.IsDefined(value))
        {
            throw new CriteriaException("sortDirection", ValidationMessages.InvalidSortDirection);
        }

        return value;
    }

    private static KeysetCursor? DecodeCursor(string? cursor, SortDirectionEnum sortDirection)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return null;
        }

        if (!KeysetCursor.TryDecode(cursor, out var decoded) || decoded!.SortDirection != sortDirection)
        {
            throw new CriteriaException("cursor", ValidationMessages.InvalidCursor);
        }

        return decoded;
    }

    private sealed class ParameterReplaceVisitor(ParameterExpression source, ParameterExpression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == source ? target : base.VisitParameter(node);
    }
}

/// <summary>The usual list: newest or oldest first by creation date.</summary>
public sealed class KeysetPaginationCriteria<T>(
    string? cursor,
    int pageSize,
    SortDirectionEnum sortDirection)
{
    public int PageSize { get; } = pageSize;
    public SortDirectionEnum SortDirection { get; } = sortDirection;

    public IQueryable<T> Apply(
        IQueryable<T> query,
        Expression<Func<T, DateTime>> createdAtSelector,
        Expression<Func<T, Guid>> idSelector)
        => By(createdAtSelector, idSelector).Apply(query);

    public KeysetPagedResult<T> ToResult(
        IReadOnlyList<T> items,
        Expression<Func<T, DateTime>> createdAtSelector,
        Expression<Func<T, Guid>> idSelector)
        => By(createdAtSelector, idSelector).ToResult(items);

    private KeysetPaginationCriteria<T, DateTime> By(
        Expression<Func<T, DateTime>> createdAtSelector,
        Expression<Func<T, Guid>> idSelector)
        => new(cursor, PageSize, SortDirection, createdAtSelector, idSelector);
}
