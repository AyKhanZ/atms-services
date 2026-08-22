using ATMS.Application.Localization;
using ATMS.Application.Models;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Data.Constants;
using ATMS.Project.Contracts.Requests.Dictionaries;
using ATMS.Project.Data.Repositories.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Project.Services.Handlers.Dictionaries;

public class GetProjectRoleDictionariesHandler(
    IRoleRepository roleRepository,
    ICacheService cache,
    IMapper mapper)
    : IRequestHandler<GetProjectRoleDictionariesRequest, DictionaryModel<Guid>[]>
{
    public async Task<DictionaryModel<Guid>[]> Handle(
        GetProjectRoleDictionariesRequest request,
        CancellationToken cancellationToken)
    {
        return await cache.GetOrSetAsync(
            key: CacheKeys.Project.AllProjectRoles(CultureHelper.CurrentLanguage),
            factory: () => GetFromDb(cancellationToken),
            ttl: CacheTtl.Dictionary,
            cancellationToken) ?? [];
    }

    private async Task<DictionaryModel<Guid>[]> GetFromDb(CancellationToken cancellationToken)
    {
        Guid[] roleIds =
        [
            RoleIds.ProjectManager,
            RoleIds.BusinessConsultant,
            RoleIds.Developer,
            RoleIds.OrgClientManager,
            RoleIds.OrgClientViewer
        ];

        var roles = await roleRepository.GetManyAsync(roleIds, cancellationToken);
        return mapper.Map<DictionaryModel<Guid>[]>(roles);
    }
}
