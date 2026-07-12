using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Me;

public class GetCurrentPermissionsHandler(
    IUserRepository userRepository,
    ICurrentUser currentUser,
    ICacheService cache) : IRequestHandler<GetCurrentPermissionsRequest, string[]>
{
    public async Task<string[]> Handle(GetCurrentPermissionsRequest request, CancellationToken cancellationToken)
    {
        var isExist = await userRepository.IsExistAsync(r => r.Id == currentUser.Id, cancellationToken);
        if (!isExist)
        {
            throw new AuthException(AuthErrorType.InvalidCredentials, LogMessages.InvalidCredentials);
        }
        
        return await cache.GetOrSetAsync(
            key: CacheKeys.Admin.UserPermissions(currentUser.Id),
            factory: () => GetFromDb(cancellationToken),
            ttl: CacheTtl.Permissions,
            cancellationToken) ?? [];
    }
    
    private async Task<string[]> GetFromDb(CancellationToken cancellationToken)
    {
        var permissions = await userRepository.GetPermissionsAsync(currentUser.Id, cancellationToken);
        return permissions.Select(p => p.Code).ToArray();
    }
}