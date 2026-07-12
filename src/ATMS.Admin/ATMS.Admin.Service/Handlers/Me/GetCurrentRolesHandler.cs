using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Models;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Me;

public class GetCurrentRolesHandler(
    IUserRepository userRepository,
    ICurrentUser currentUser,
    IMapper mapper,
    ICacheService cache) : IRequestHandler<GetCurrentRolesRequest, DictionaryModel<Guid>[]>
{
    public async Task<DictionaryModel<Guid>[]> Handle(GetCurrentRolesRequest request, CancellationToken cancellationToken)
    {
        var isExist = await userRepository.IsExistAsync(r => r.Id == currentUser.Id, cancellationToken);
        if (!isExist)
        {
            throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);
        }
        
        return await cache.GetOrSetAsync(
            key: CacheKeys.Admin.UserRoles(currentUser.Id),
            factory: () => GetFromDb(cancellationToken),
            ttl: CacheTtl.Roles,
            cancellationToken) ?? [];
    }
    
    private async Task<DictionaryModel<Guid>[]> GetFromDb(CancellationToken cancellationToken)
    {
        var roles = await userRepository.GetRolesAsync(currentUser.Id, cancellationToken);
        return mapper.Map<DictionaryModel<Guid>[]>(roles);
    }
}