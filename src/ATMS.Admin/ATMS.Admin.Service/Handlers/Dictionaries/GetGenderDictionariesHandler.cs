using ATMS.Admin.Contracts.Requests.Dictionaries;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Localization;
using ATMS.Application.Models;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Dictionaries;

public class GetGenderDictionariesHandler(
    IDictionariesRepository dictionariesRepository,
    ICacheService cache)
    : IRequestHandler<GetGenderDictionariesRequest, DictionaryModel[]>
{
    public async Task<DictionaryModel[]> Handle(
        GetGenderDictionariesRequest request,
        CancellationToken cancellationToken)
    {
        return await cache.GetOrSetAsync(
            key: CacheKeys.Admin.AllGenders(CultureHelper.CurrentLanguage),
            factory: () => GetFromDb(CultureHelper.CurrentLanguage, cancellationToken),
            ttl: CacheTtl.Dictionary,
            cancellationToken) ?? [];
    }
    
    private async Task<DictionaryModel[]> GetFromDb(string language, CancellationToken cancellationToken)
    {
        var genders = await dictionariesRepository.GetGendersAsync(cancellationToken);
        return genders.Select(g => g.ToDictionaryModel(g.Translations, language)).ToArray();
    }
}