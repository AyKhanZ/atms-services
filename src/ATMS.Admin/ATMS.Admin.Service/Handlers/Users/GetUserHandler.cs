using ATMS.Admin.Contracts.Models.Organizations;
using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Contracts.Requests.Users;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Providers.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Localization;
using ATMS.Application.Models;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ATMS.Admin.Service.Handlers.Users;

public sealed class GetUserHandler(
    IUserRepository userRepository,
    IOrganizationProvider organizationProvider,
    IMapper mapper,
    ICacheService cache,
    ILogger<GetUserHandler> logger
    ) : IRequestHandler<GetUserRequest, UserModel>
{
    public async Task<UserModel> Handle(GetUserRequest request, CancellationToken cancellationToken)
    {
        return await cache.GetOrSetAsync(
                   key: CacheKeys.Admin.UserById(request.Id, CultureHelper.CurrentLanguage),
                   factory: () => GetFromDb(request.Id, CultureHelper.CurrentLanguage, cancellationToken),
                   ttl: CacheTtl.Entity,
                   cancellationToken)
               ?? throw new EntityException(EntityErrorTypeEnum.NotFound, AccountMessages.UserNotFound);
    }

    private async Task<UserModel> GetFromDb(Guid id, string language, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(id, cancellationToken)
                   ?? throw new EntityException(EntityErrorTypeEnum.NotFound, AccountMessages.UserNotFound);

        var model = mapper.Map<UserModel>(user);
        model.Gender = user.Gender.ToDictionaryModel(user.Gender.Translations, language);
        model.MaritalStatus = user.MaritalStatus.ToDictionaryModel(user.MaritalStatus.Translations, language);
        model.UserStatus = user.UserStatus.ToDictionaryModel(user.UserStatus.Translations, language);
        model.Roles = user.UserRoles
            .Select(ur => mapper.Map<DictionaryModel<Guid>>(ur.Role))
            .ToArray();

        if (user.OrganizationId.HasValue)
        {
            model.Organization = await GetOrganizationAsync(user.OrganizationId.Value, cancellationToken);
        }

        return model;
    }

    private async Task<OrganizationModel?> GetOrganizationAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return await organizationProvider.GetAsync(id, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException
                                          && !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Could not load organization {OrganizationId} from Project.", id);
            return null;
        }
    }
}
