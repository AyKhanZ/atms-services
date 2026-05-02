using ATMS.Admin.Contracts.Models;
using ATMS.Admin.Contracts.Requests.Roles;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Entity;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Roles;

public class GetRoleHandler(
    IRoleRepository roleRepository,
    IMapper mapper,
    ICacheService cache) : IRequestHandler<GetRoleRequest, RoleModel>
{
    public async Task<RoleModel> Handle(GetRoleRequest request, CancellationToken cancellationToken)
    {
        return await cache.GetOrSetAsync(
                   key: CacheKeys.Admin.RoleById(request.Id),
                   factory: () => GetFromDb(request.Id, cancellationToken),
                   ttl: CacheTtl.Roles,
                   cancellationToken)
               ?? throw new EntityException(EntityErrorType.NotFound, RoleMessages.NotFound);
    }
    
    private async Task<RoleModel> GetFromDb(Guid id, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetAsync(r => r.Id == id, cancellationToken)
                   ?? throw new EntityException(EntityErrorType.NotFound, RoleMessages.NotFound);

        return mapper.Map<RoleModel>(role);
    }
}