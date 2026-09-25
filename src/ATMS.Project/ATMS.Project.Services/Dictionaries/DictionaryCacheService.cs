using ATMS.Application.Localization;
using ATMS.Application.Models;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data;
using ATMS.Data.Interfaces;
using ATMS.Project.Data.Repositories.Interfaces;
using ATMS.Project.Services.Dictionaries.Interfaces;

namespace ATMS.Project.Services.Dictionaries;

// Statuses, types and priorities in the caller's language, from Redis and from the database only on
// a miss. The dictionary endpoints and the history read them the same way: before, each of the eight
// handlers carried its own copy of this, and the history went to the database on every page.
public sealed class DictionaryCacheService(
    IDictionariesRepository dictionariesRepository,
    ICacheService cache) : IDictionaryCacheService
{
    public Task<DictionaryModel[]> GetProjectKindsAsync(CancellationToken cancellationToken) =>
        GetAsync(
            CacheKeys.Project.AllProjectKinds,
            dictionariesRepository.GetProjectKindsAsync,
            kind => kind.Translations,
            cancellationToken);

    public Task<DictionaryModel[]> GetProjectStatusesAsync(CancellationToken cancellationToken) =>
        GetAsync(
            CacheKeys.Project.AllProjectStatuses,
            dictionariesRepository.GetProjectStatusesAsync,
            status => status.Translations,
            cancellationToken);

    public Task<DictionaryModel[]> GetProjectTypesAsync(CancellationToken cancellationToken) =>
        GetAsync(
            CacheKeys.Project.AllProjectTypes,
            dictionariesRepository.GetProjectTypesAsync,
            type => type.Translations,
            cancellationToken);

    public Task<DictionaryModel[]> GetWorkGroupStatusesAsync(CancellationToken cancellationToken) =>
        GetAsync(
            CacheKeys.Project.AllWorkGroupStatuses,
            dictionariesRepository.GetWorkGroupStatusesAsync,
            status => status.Translations,
            cancellationToken);

    public Task<DictionaryModel[]> GetWorkItemPrioritiesAsync(CancellationToken cancellationToken) =>
        GetAsync(
            CacheKeys.Project.AllWorkItemPriorities,
            dictionariesRepository.GetWorkItemPrioritiesAsync,
            priority => priority.Translations,
            cancellationToken);

    public Task<DictionaryModel[]> GetWorkTaskStatusesAsync(CancellationToken cancellationToken) =>
        GetAsync(
            CacheKeys.Project.AllWorkTaskStatuses,
            dictionariesRepository.GetWorkTaskStatusesAsync,
            status => status.Translations,
            cancellationToken);

    public Task<DictionaryModel[]> GetWorkTicketStatusesAsync(CancellationToken cancellationToken) =>
        GetAsync(
            CacheKeys.Project.AllWorkTicketStatuses,
            dictionariesRepository.GetWorkTicketStatusesAsync,
            status => status.Translations,
            cancellationToken);

    public Task<DictionaryModel[]> GetWorkTicketTypesAsync(CancellationToken cancellationToken) =>
        GetAsync(
            CacheKeys.Project.AllWorkTicketTypes,
            dictionariesRepository.GetWorkTicketTypesAsync,
            type => type.Translations,
            cancellationToken);

    private async Task<DictionaryModel[]> GetAsync<T>(
        Func<string, string> key,
        Func<CancellationToken, Task<List<T>>> load,
        Func<T, IEnumerable<ITranslation>> translations,
        CancellationToken cancellationToken)
        where T : TranslatableDictionaryEntity
    {
        var language = CultureHelper.CurrentLanguage;

        return await cache.GetOrSetAsync(
            key: key(language),
            factory: async () =>
            {
                var items = await load(cancellationToken);
                return items.Select(item => item.ToDictionaryModel(translations(item), language)).ToArray();
            },
            ttl: CacheTtl.Dictionary,
            cancellationToken) ?? [];
    }
}
