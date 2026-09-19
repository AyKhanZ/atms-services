using ATMS.Data.Enums;
using ATMS.Project.Data.Models.Search;

namespace ATMS.Project.Data.Repositories.Interfaces;

public interface IGlobalSearchRecentRepository
{
    Task<GlobalSearchRow[]> GetRecentAsync(
        Guid userId, bool isSuperAdmin, string language, CancellationToken cancellationToken);

    Task<bool> RecordRecentAsync(
        Guid userId, bool isSuperAdmin, GlobalSearchItemType itemType, Guid itemId, CancellationToken cancellationToken);
}
