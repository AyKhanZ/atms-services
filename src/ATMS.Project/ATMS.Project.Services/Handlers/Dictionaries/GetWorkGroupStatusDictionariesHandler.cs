using ATMS.Application.Localization;
using ATMS.Application.Models;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetWorkGroupStatusDictionariesHandler(
    IDictionariesRepository dictionariesRepository,
    ICacheService cache)
    : IRequestHandler<GetWorkGroupStatusDictionariesRequest, DictionaryModel[]>
{
    public async Task<DictionaryModel[]> Handle(GetWorkGroupStatusDictionariesRequest request, CancellationToken cancellationToken)
    {
        return await cache.GetOrSetAsync(
            key: CacheKeys.Project.AllWorkGroupStatuses(CultureHelper.CurrentLanguage),
            factory: () => GetFromDb(CultureHelper.CurrentLanguage, cancellationToken),
            ttl: CacheTtl.Dictionary,
            cancellationToken) ?? [];
    }
    
    private async Task<DictionaryModel[]> GetFromDb(string language, CancellationToken cancellationToken)
    {
        var genders = await dictionariesRepository.GetWorkGroupStatusesAsync(cancellationToken);
        return genders.Select(g => g.ToDictionaryModel(g.Translations, language)).ToArray();
    }
}