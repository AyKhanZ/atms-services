using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Localization;
using ATMS.Application.Models;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Users;

public class GetUserHandler(
    IUserRepository userRepository,
    IMapper mapper,
    ICacheService cache
    ) : IRequestHandler<GetUserRequest, UserModel>
{
    public async Task<UserModel> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        return await cache.GetOrSetAsync(
                   key: CacheKeys.Admin.UserById(request.Id, CultureHelper.CurrentLanguage),
                   factory: () => GetFromDb(request.Id, CultureHelper.CurrentLanguage, cancellationToken),
                   ttl: CacheTtl.Entity,
                   cancellationToken)
               ?? throw new EntityException(EntityErrorType.NotFound, AccountMessages.UserNotFound);
    }
    
    private async Task<UserModel> GetFromDb(Guid id, string language, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(id, cancellationToken)
                   ?? throw new EntityException(EntityErrorType.NotFound, AccountMessages.UserNotFound);

        var model = mapper.Map<UserModel>(user);
        model.Gender = user.Gender.ToDictionaryModel(user.Gender.Translations, language);
        model.MaritalStatus = user.MaritalStatus.ToDictionaryModel(user.MaritalStatus.Translations, language);
        model.UserStatus = user.UserStatus.ToDictionaryModel(user.UserStatus.Translations, language);
        model.Roles = user.UserRoles
            .Select(ur => mapper.Map<DictionaryModel<Guid>>(ur.Role))
            .ToArray();

        return model;
    }
}