using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Localization;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Profile;

public class UpdateLanguageHandler(
    IUserRepository userRepository,
    IDictionariesRepository dictionariesRepository,
    ICacheService cache) : IRequestHandler<UpdateLanguageCommand>
{
    public async Task Handle(UpdateLanguageCommand command, CancellationToken cancellationToken)
    {
        var entity = await userRepository.FindAsync(u => u.Id == command.Id, cancellationToken);
        if (entity == null)
        {
            throw new EntityException(EntityErrorType.NotFound, AccountMessages.UserNotFound);
        }
        var allLanguages = await dictionariesRepository.GetLanguagesAsync(cancellationToken);
        var language = allLanguages.FirstOrDefault(x =>
                            string.Equals(x.Code, command.Language, StringComparison.OrdinalIgnoreCase))
                       ?? throw new EntityException(EntityErrorType.NotFound, ProfileMessages.LanguageNotSupported);

        entity.LanguageId = language.Id;

        await userRepository.SaveAsync(cancellationToken);

        await InvalidateUserCacheAsync(command, cancellationToken);
    }

    private async Task InvalidateUserCacheAsync(
        UpdateLanguageCommand command,
        CancellationToken cancellationToken)
    {
        foreach (var language in SupportedLanguages.All)
        {
            await cache.RemoveAsync(CacheKeys.Admin.UserById(command.Id, language), cancellationToken);
        }

        await cache.RemoveAsync(CacheKeys.Admin.MeById(command.Id), cancellationToken);
    }
}
