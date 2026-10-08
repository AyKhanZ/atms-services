using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Localization;
using ATMS.Application.Interfaces;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Profile;

public sealed class UpdateLanguageHandler(
    ICurrentUser currentUser,
    IUserRepository userRepository,
    IDictionariesRepository dictionariesRepository,
    ICacheService cache) : IRequestHandler<UpdateLanguageCommand>
{
    public async Task Handle(UpdateLanguageCommand command, CancellationToken cancellationToken)
    {
        var entity = await userRepository.FindAsync(u => u.Id == currentUser.Id, cancellationToken);
        if (entity == null)
        {
            throw new EntityException(EntityErrorTypeEnum.NotFound, AccountMessages.UserNotFound);
        }
        var allLanguages = await dictionariesRepository.GetLanguagesAsync(cancellationToken);
        var language = allLanguages.FirstOrDefault(x =>
                            string.Equals(x.Code, command.Language, StringComparison.OrdinalIgnoreCase))
                       ?? throw new EntityException(EntityErrorTypeEnum.NotFound, ProfileMessages.LanguageNotSupported);

        entity.LanguageId = language.Id;

        await userRepository.SaveAsync(cancellationToken);

        await InvalidateUserCacheAsync(currentUser.Id, cancellationToken);
    }

    private async Task InvalidateUserCacheAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        foreach (var language in SupportedLanguages.All)
        {
            await cache.RemoveAsync(CacheKeys.Admin.UserById(userId, language), cancellationToken);
        }

        await cache.RemoveAsync(CacheKeys.Admin.MeById(userId), cancellationToken);
        await cache.RemoveAsync(CacheKeys.Admin.ProfileById(userId), cancellationToken);
    }
}
