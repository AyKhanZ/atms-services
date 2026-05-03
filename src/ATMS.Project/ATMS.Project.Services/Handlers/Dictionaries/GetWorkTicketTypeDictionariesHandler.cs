using ATMS.Application.Localization;
using ATMS.Application.Models;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Data.Repositories.Interfaces;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetWorkTicketTypeDictionariesHandler(
    IDictionariesRepository dictionariesRepository,
    ICacheService cache)
    : IRequestHandler<GetWorkTicketTypeDictionariesRequest, DictionaryModel[]>
{
    public async Task<DictionaryModel[]> Handle(GetWorkTicketTypeDictionariesRequest request, CancellationToken cancellationToken)
    {
        return await cache.GetOrSetAsync(
            key: CacheKeys.Project.AllWorkTicketTypes(CultureHelper.CurrentLanguage),
            factory: () => GetFromDb(CultureHelper.CurrentLanguage, cancellationToken),
            ttl: CacheTtl.Dictionary,
            cancellationToken) ?? [];
    }
    
    private async Task<DictionaryModel[]> GetFromDb(string language, CancellationToken cancellationToken)
    {
        var dictionaries = await dictionariesRepository.GetWorkTicketTypesAsync(cancellationToken);
        return dictionaries.Select(g => g.ToDictionaryModel(g.Translations, language)).ToArray();
    }
}