using ATMS.Application.Localization;
using ATMS.Application.Models;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetWorkItemPriorityDictionariesHandler(
    IDictionariesRepository dictionariesRepository,
    ICacheService cache)
    : IRequestHandler<GetWorkItemPriorityDictionariesRequest, DictionaryModel[]>
{
    public async Task<DictionaryModel[]> Handle(GetWorkItemPriorityDictionariesRequest request, CancellationToken cancellationToken)
    {
        return await cache.GetOrSetAsync(
            key: CacheKeys.Project.AllWorkItemPriorities(CultureHelper.CurrentLanguage),
            factory: () => GetFromDb(CultureHelper.CurrentLanguage, cancellationToken),
            ttl: CacheTtl.Dictionary,
            cancellationToken) ?? [];
    }
    
    private async Task<DictionaryModel[]> GetFromDb(string language, CancellationToken cancellationToken)
    {
        var dictionaries = await dictionariesRepository.GetWorkItemPrioritiesAsync(cancellationToken);
        return dictionaries.Select(g => g.ToDictionaryModel(g.Translations, language)).ToArray();
    }
}