using ATMS.Admin.Contracts.Commands.Users;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Interfaces;
using ATMS.Application.Localization;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Contracts.Events.Users;
using ATMS.Data.Enums;
using ATMS.Data.Messaging;
using ATMS.Messaging.Configuration;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Users;

public sealed class UpdateUserStatusHandler(
    ICurrentUser currentUser,
    IUserRepository userRepository,
    IUserSessionRepository userSessionRepository,
    IOutboxRepository outboxRepository,
    ICacheService cache) : IRequestHandler<UpdateUserStatusCommand>
{
    public async Task Handle(UpdateUserStatusCommand command, CancellationToken cancellationToken)
    {
        if (command.Id == currentUser.Id)
        {
            throw new AuthException(AuthErrorTypeEnum.Forbidden, AccountMessages.CannotChangeOwnStatus);
        }

        var entity = await userRepository.FindAsync(u => u.Id == command.Id, cancellationToken);
        if (entity == null)
        {
            throw new EntityException(EntityErrorTypeEnum.NotFound, AccountMessages.UserNotFound);
        }

        var roles = await userRepository.GetRolesAsync(entity.Id, cancellationToken);
        if (roles.Any(role => role.UserType == (int)UserTypeEnum.SuperAdmin))
        {
            throw new AuthException(AuthErrorTypeEnum.Forbidden, AccountMessages.CannotChangeSuperAdminStatus);
        }

        // same status again, a double click: no event, no session revoke
        if (entity.UserStatusId == command.UserStatusId)
        {
            return;
        }

        entity.UserStatusId = command.UserStatusId;
        // a timed lock must not outlive the status an administrator just chose
        entity.LockoutEnd = null;
        entity.FailedLoginCount = 0;

        await outboxRepository.AddAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserStatusChanged,
            new UserStatusChangedEvent(
                entity.Id,
                entity.UserStatusId == (int)UserStatusEnum.Active),
            cancellationToken);

        await userRepository.SaveAsync(cancellationToken);

        if (entity.UserStatusId != (int)UserStatusEnum.Active)
        {
            await userSessionRepository.RevokeAllAsync(
                entity.Id,
                DateTime.UtcNow,
                cancellationToken);
        }

        await InvalidateUserCacheAsync(command, cancellationToken);
    }

    private async Task InvalidateUserCacheAsync(
        UpdateUserStatusCommand command,
        CancellationToken cancellationToken)
    {
        foreach (var language in SupportedLanguages.All)
        {
            await cache.RemoveAsync(CacheKeys.Admin.UserById(command.Id, language), cancellationToken);
        }

        await cache.RemoveAsync(CacheKeys.Admin.MeById(command.Id), cancellationToken);
    }
}
