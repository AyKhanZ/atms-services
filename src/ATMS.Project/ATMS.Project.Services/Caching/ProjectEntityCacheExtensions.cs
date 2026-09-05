using ATMS.Application.Localization;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;

namespace ATMS.Project.Services.Caching;

internal static class ProjectEntityCacheExtensions
{
    internal static Task RemoveWorkProjectAsync(this ICacheService cache, Guid projectId, CancellationToken cancellationToken)
    {
        return cache.RemoveLocalizedEntriesAsync(
            language => CacheKeys.Project.ProjectById(projectId, language),
            cancellationToken);
    }

    internal static Task RemoveWorkTicketAsync(this ICacheService cache, Guid workTicketId, CancellationToken cancellationToken)
    {
        return cache.RemoveLocalizedEntriesAsync(
            language => CacheKeys.Project.TicketById(workTicketId, language),
            cancellationToken);
    }

    internal static async Task RemoveWorkTicketsAsync(
        this ICacheService cache,
        IEnumerable<Guid> workTicketIds,
        CancellationToken cancellationToken)
    {
        foreach (var workTicketId in workTicketIds.Distinct())
        {
            await cache.RemoveWorkTicketAsync(workTicketId, cancellationToken);
        }
    }

    internal static Task RemoveWorkTaskAsync(this ICacheService cache, Guid workTaskId, CancellationToken cancellationToken)
    {
        return cache.RemoveLocalizedEntriesAsync(
            language => CacheKeys.Project.TaskById(workTaskId, language),
            cancellationToken);
    }

    internal static async Task RemoveWorkTasksAsync(
        this ICacheService cache,
        IEnumerable<Guid> workTaskIds,
        CancellationToken cancellationToken)
    {
        foreach (var workTaskId in workTaskIds.Distinct())
        {
            await cache.RemoveWorkTaskAsync(workTaskId, cancellationToken);
        }
    }

    private static async Task RemoveLocalizedEntriesAsync(this ICacheService cache, Func<string, string> keyFactory, CancellationToken cancellationToken)
    {
        foreach (var language in SupportedLanguages.All)
        {
            await cache.RemoveAsync(keyFactory(language), cancellationToken);
        }
    }
}
