using ATMS.Admin.Contracts.Commands.Profile;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Resources;
using ATMS.Application.Exceptions.Entity;
using ATMS.Contracts.Events.Users;
using ATMS.Messaging.Configuration;
using ATMS.Messaging.Interfaces;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Profile;

public class UpdateSettingsHandler(
    IUserRepository userRepository,
    IMessagePublisher messagePublisher) : IRequestHandler<UpdateSettingsCommand>
{
    public async Task Handle(UpdateSettingsCommand command, CancellationToken cancellationToken)
    {
        var entity = await userRepository.FindAsync(u => u.Id == command.Id, cancellationToken);
        if (entity == null)
        {
            throw new EntityException(EntityErrorType.NotFound, AccountMessages.UserNotFound);
        }

        entity.Name = command.Name;
        entity.Surname = command.Surname;
        entity.PhoneNumber = command.PhoneNumber;
        entity.BirthDate = command.BirthDate;
        entity.Position = command.Position;
        entity.MaritalStatusId = command.MaritalStatusId;
        entity.GenderId = command.GenderId;

        await userRepository.SaveAsync(cancellationToken);
        
        var @event = new UserUpdatedEvent(
            entity.Id,
            entity.Name,
            entity.Surname,
            entity.AvatarPath);

        await messagePublisher.PublishAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserUpdated,
            @event,
            cancellationToken);
    }
}