using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Contracts.Models.Profile;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Entity;
using ATMS.Application.Exceptions.Enums;
using ATMS.Application.Interfaces;
using ATMS.Application.Localization;
using ATMS.Data.Constants;
using ATMS.Caching.Constants;
using ATMS.Caching.Services.Interfaces;
using ATMS.Contracts.Events.Users;
using ATMS.Data.Messaging;
using ATMS.Infrastructure.Enums;
using ATMS.Infrastructure.Images;
using ATMS.Messaging.Configuration;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ATMS.Admin.Service.Handlers.Profile;

public sealed class UpdateSettingsHandler(
    ICurrentUser currentUser,
    IUserRepository userRepository,
    IOutboxRepository outboxRepository,
    IImageStorage imageStorage,
    ICacheService cache,
    IMapper mapper,
    ILogger<UpdateSettingsHandler> logger) : IRequestHandler<UpdateSettingsCommand, ProfileModel>
{
    public async Task<ProfileModel> Handle(UpdateSettingsCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindAsync(u => u.Id == currentUser.Id, cancellationToken)
            ?? throw new EntityException(EntityErrorTypeEnum.NotFound, AccountMessages.UserNotFound);

        var oldAvatarPath = user.AvatarPath;
        string? newAvatarPath = null;

        if (command.Avatar is not null)
        {
            var image = await imageStorage.SaveAsync(
                command.Avatar,
                ImageStorageFolderEnum.Users,
                currentUser.Id,
                cancellationToken);
            newAvatarPath = image.RelativePath;
        }

        user.Name = command.Name;
        user.Surname = command.Surname;
        user.PhoneNumber = command.PhoneNumber;
        user.BirthDate = command.BirthDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        user.Position = command.Position;
        user.MaritalStatusId = command.MaritalStatusId;
        user.GenderId = command.GenderId;
        user.LanguageId = command.LanguageId;
        user.AvatarPath = newAvatarPath ?? oldAvatarPath;

        try
        {
            await outboxRepository.AddAsync(
                MessagingConstants.Exchanges.UserEvents,
                MessagingConstants.RoutingKeys.UserUpdated,
                new UserUpdatedEvent(
                    user.Id,
                    user.Name,
                    user.Surname,
                    user.AvatarPath,
                    user.HasCompletedOnboarding,
                    user.Position),
                cancellationToken);

            await userRepository.SaveAsync(cancellationToken);
        }
        catch
        {
            if (newAvatarPath is not null)
            {
                await imageStorage.DeleteAsync(newAvatarPath, CancellationToken.None);
            }

            throw;
        }

        foreach (var language in SupportedLanguages.All)
        {
            await cache.RemoveAsync(CacheKeys.Admin.UserById(user.Id, language), cancellationToken);
        }

        await cache.RemoveAsync(CacheKeys.Admin.MeById(user.Id), cancellationToken);
        await cache.RemoveAsync(CacheKeys.Admin.ProfileById(user.Id), cancellationToken);

        if (newAvatarPath is not null &&
            !string.IsNullOrWhiteSpace(oldAvatarPath) &&
            oldAvatarPath != DefaultValues.UserAvatar &&
            oldAvatarPath != newAvatarPath)
        {
            try
            {
                await imageStorage.DeleteAsync(oldAvatarPath, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Could not delete the previous profile avatar for user {UserId}.", user.Id);
            }
        }

        return mapper.Map<ProfileModel>(user);
    }
}
