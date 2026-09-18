using ATMS.Data.Criteria;
using ATMS.Data.Enums;
using ATMS.Project.Data.Models.Search;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IGlobalSearchRepository
{
    Task<GlobalSearchRow[]> SearchAsync(
        Guid userId, bool isSuperAdmin, string search, int take, string language, CancellationToken cancellationToken);

    Task<GlobalSearchRow[]> SearchPageAsync(
        Guid userId,
        bool isSuperAdmin,
        string search,
        GlobalSearchItemType itemType,
        KeysetCursor? cursor,
        SortDirectionEnum sortDirection,
        int pageSize,
        string language,
        CancellationToken cancellationToken);
}
