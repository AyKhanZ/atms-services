using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Roles;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Roles;

public class GetRolesHandler(
    IRoleRepository roleRepository,
    IMapper mapper,
    ICacheService cache) : IRequestHandler<GetRolesRequest, RoleModel[]>
{
    public async Task<RoleModel[]> Handle(GetRolesRequest request, CancellationToken cancellationToken)
    {
        return await cache.GetOrSetAsync(
            key: CacheKeys.Admin.AllRoles,
            factory: () => GetFromDb(cancellationToken),
            ttl: CacheTtl.Roles,
            cancellationToken) ?? [];
    }
    
    private async Task<RoleModel[]> GetFromDb(CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAsync(cancellationToken);
        return mapper.Map<RoleModel[]>(roles);
    }
}