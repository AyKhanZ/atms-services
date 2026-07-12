using ATMS.Admin.Contracts.Requests.Dictionaries;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Models;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Dictionaries;

public class GetRoleDictionariesHandler(
    IRoleRepository roleRepository,
    ICacheService cache)
    : IRequestHandler<GetRoleDictionariesRequest, DictionaryModel<Guid>[]>
{
    public async Task<DictionaryModel<Guid>[]> Handle(
        GetRoleDictionariesRequest request,
        CancellationToken cancellationToken)
    {
        return await cache.GetOrSetAsync(
            key: CacheKeys.Admin.AllRoles,
            factory: () => GetFromDb(cancellationToken),
            ttl: CacheTtl.Dictionary,
            cancellationToken) ?? [];
    }

    private async Task<DictionaryModel<Guid>[]> GetFromDb(CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAsync(cancellationToken);
        return roles
            .OrderBy(r => r.Name)
            .Select(r => new DictionaryModel<Guid>
            {
                Id = r.Id,
                Name = r.Name,
                Code = r.Name
            })
            .ToArray();
    }
}