using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Localization;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Contracts.Events.Users;
using ATMS.Messaging.Configuration;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Profile;

public class UpdatePhotoHandler(
    IUserRepository userRepository,
    IOutboxRepository outboxRepository,
    ICacheService cache) : IRequestHandler<UpdatePhotoCommand>
{
    public async Task Handle(UpdatePhotoCommand command, CancellationToken cancellationToken)
    {
        var entity = await userRepository.FindAsync(u => u.Id == command.Id, cancellationToken);
        if (entity == null)
        {
            throw new EntityException(EntityErrorType.NotFound, AccountMessages.UserNotFound);
        }
        
        entity.AvatarPath = command.FileName;
        
        var @event = new UserUpdatedEvent(
            entity.Id,
            entity.Name,
            entity.Surname,
            entity.AvatarPath);

        await outboxRepository.AddAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserUpdated,
            @event,
            cancellationToken);

        await userRepository.SaveAsync(cancellationToken);
        
        await InvalidateUserCacheAsync(command, cancellationToken);
    }
    
    private async Task InvalidateUserCacheAsync(
        UpdatePhotoCommand command,
        CancellationToken cancellationToken)
    {
        foreach (var language in SupportedLanguages.All)
        {
            await cache.RemoveAsync(CacheKeys.Admin.UserById(command.Id, language), cancellationToken);
        }

        await cache.RemoveAsync(CacheKeys.Admin.MeById(command.Id), cancellationToken);
    }
}
