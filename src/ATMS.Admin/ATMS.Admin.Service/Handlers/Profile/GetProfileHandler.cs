using ATMS.Admin.Contracts.Models.Profile;
using ATMS.Admin.Contracts.Requests.Profile;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Interfaces;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Profile;

public sealed class GetProfileHandler(
    ICurrentUser currentUser,
    IUserRepository userRepository,
    ICacheService cache,
    IMapper mapper) : IRequestHandler<GetProfileRequest, ProfileModel>
{
    public async Task<ProfileModel> Handle(GetProfileRequest request, CancellationToken cancellationToken)
    {
        return await cache.GetOrSetAsync(
                   key: CacheKeys.Admin.ProfileById(currentUser.Id),
                   factory: async () =>
                   {
                       var user = await userRepository.GetAsync(currentUser.Id, cancellationToken)
                           ?? throw new EntityException(EntityErrorType.NotFound, AccountMessages.UserNotFound);

                       return mapper.Map<ProfileModel>(user);
                   },
                   ttl: CacheTtl.Entity,
                   cancellationToken)
               ?? throw new EntityException(EntityErrorType.NotFound, AccountMessages.UserNotFound);
    }
}
