using ATMS.Admin.Contracts.Models.Dictionaries;
using ATMS.Admin.Contracts.Requests.Dictionaries;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Localization;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Dictionaries;

public class GetPermissionDictionariesHandler(
    IPermissionRepository permissionRepository,
    ICacheService cache)
    : IRequestHandler<GetPermissionDictionariesRequest, PermissionModel[]>
{
    public async Task<PermissionModel[]> Handle(
        GetPermissionDictionariesRequest request,
        CancellationToken cancellationToken)
    {
        return await cache.GetOrSetAsync(
            key: CacheKeys.Admin.AllPermissions(CultureHelper.CurrentLanguage),
            factory: () => GetFromDb(CultureHelper.CurrentLanguage, cancellationToken),
            ttl: CacheTtl.Permissions,
            cancellationToken) ?? [];
    }

    private async Task<PermissionModel[]> GetFromDb(string language, CancellationToken cancellationToken)
    {
        var permissions = await permissionRepository.GetAsync(cancellationToken);
        return permissions.Select(p => new PermissionModel
        {
            Id = p.Id,
            Code = p.Code,
            Module = p.Module,
            Name = p.Translations.Resolve(language, p.Code)
        }).ToArray();
    }
}