using ATMS.Admin.Contracts.Requests.Dictionaries;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Localization;
using ATMS.Application.Models;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Dictionaries;

public class GetUserStatusDictionariesHandler(
    IDictionariesRepository dictionariesRepository,
    ICacheService cache)
    : IRequestHandler<GetUserStatusDictionariesRequest, DictionaryModel[]>
{
    public async Task<DictionaryModel[]> Handle(
        GetUserStatusDictionariesRequest request,
        CancellationToken cancellationToken)
    {
        return await cache.GetOrSetAsync(
            key: CacheKeys.Admin.AllUserStatuses(CultureHelper.CurrentLanguage),
            factory: () => GetFromDb(CultureHelper.CurrentLanguage, cancellationToken),
            ttl: CacheTtl.Roles,
            cancellationToken) ?? [];
    }

    private async Task<DictionaryModel[]> GetFromDb(string language, CancellationToken cancellationToken)
    {
        var userStatuses = await dictionariesRepository.GetUserStatusesAsync(cancellationToken);
        return userStatuses.Select(g => g.ToDictionaryModel(g.Translations, language)).ToArray();
    }
}