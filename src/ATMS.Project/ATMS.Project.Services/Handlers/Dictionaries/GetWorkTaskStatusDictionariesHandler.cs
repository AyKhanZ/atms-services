using ATMS.Application.Localization;
using ATMS.Application.Models;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetWorkTaskStatusDictionariesHandler(
    IDictionariesRepository dictionariesRepository,
    ICacheService cache)
    : IRequestHandler<GetWorkTaskStatusDictionariesRequest, DictionaryModel[]>
{
    public async Task<DictionaryModel[]> Handle(GetWorkTaskStatusDictionariesRequest request, CancellationToken cancellationToken)
    {
        return await cache.GetOrSetAsync(
            key: CacheKeys.Project.AllWorkTaskStatuses(CultureHelper.CurrentLanguage),
            factory: () => GetFromDb(CultureHelper.CurrentLanguage, cancellationToken),
            ttl: CacheTtl.Dictionary,
            cancellationToken) ?? [];
    }
    
    private async Task<DictionaryModel[]> GetFromDb(string language, CancellationToken cancellationToken)
    {
        var genders = await dictionariesRepository.GetWorkTaskStatusesAsync(cancellationToken);
        return genders.Select(g => g.ToDictionaryModel(g.Translations, language)).ToArray();
    }
}