using ATMS.Application.Localization;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Models.BoardColumns;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Domain.Board.Interfaces;
using ATMS.Project.Services.Domain.Dictionaries.Interfaces;

namespace ATMS.Project.Services.Domain.Board;

public sealed class BoardColumnCacheService(
    IBoardColumnRepository boardColumns,
    IDictionaryCacheService dictionaries,
    ICacheService cache) : IBoardColumnCacheService
{
    // columns change only with a migration, the TTL is enough
    public async Task<BoardColumnModel[]> GetAllAsync(CancellationToken cancellationToken)
    {
        var language = CultureHelper.CurrentLanguage;

        return await cache.GetOrSetAsync(
            key: CacheKeys.Project.AllBoardColumns(language),
            factory: async () =>
            {
                var columns = await boardColumns.GetOrderedAsync(cancellationToken);
                var statuses = (await dictionaries.GetWorkTaskStatusesAsync(cancellationToken))
                    .ToDictionary(status => status.Id, status => status.Name);
                var split = columns
                    .GroupBy(column => column.Category)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key)
                    .ToHashSet();

                return columns.Select(column =>
                {
                    var name = column.Translations.Resolve(language, column.Code);
                    return new BoardColumnModel
                    {
                        Id = column.Id,
                        Code = column.Code,
                        Name = name,
                        Label = split.Contains(column.Category) && statuses.TryGetValue(column.Category, out var status)
                            ? $"{status} · {name}"
                            : name,
                        Category = column.Category,
                        Order = column.Order
                    };
                }).ToArray();
            },
            ttl: CacheTtl.Dictionary,
            cancellationToken) ?? [];
    }
}
