using ATMS.Admin.Contracts.Models.Me;
using ATMS.Admin.Contracts.Requests.Me;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Me;

public class GetMeHandler(
    IUserRepository userRepository,
    ICurrentUser currentUser,
    IMapper mapper,
    ICacheService cache)
    : IRequestHandler<GetMeRequest, MeModel>
{
    public async Task<MeModel> Handle(GetMeRequest request, CancellationToken cancellationToken)
    {
        var key = CacheKeys.Admin.MeById(currentUser.Id);
        var model = await cache.GetOrSetAsync(
                   key: key,
                   factory: () => GetFromDb(cancellationToken),
                   ttl: CacheTtl.Entity,
                   cancellationToken);

        if (model?.Language is not null)
        {
            return model;
        }

        await cache.RemoveAsync(key, cancellationToken);

        return await GetFromDb(cancellationToken);
    }

    private async Task<MeModel> GetFromDb(CancellationToken cancellationToken)
    {
        var user = await userRepository.GetMeAsync(currentUser.Id, cancellationToken)
                   ?? throw new AuthException(AuthErrorType.InvalidCredentials,
                       LogMessages.InvalidCredentials);

        return mapper.Map<MeModel>(user);
    }
}
