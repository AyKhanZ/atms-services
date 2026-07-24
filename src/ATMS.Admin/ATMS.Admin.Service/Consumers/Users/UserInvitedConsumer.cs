using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Contracts.Events.Users;
using ATMS.Data.Constants;
using ATMS.Messaging.Configuration;
using ATMS.Messaging.Infrastructure;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ATMS.Admin.Service.Consumers.Users;

public class UserInvitedConsumer(
    RabbitMqConnectionFactory connectionFactory,
    IServiceScopeFactory scopeFactory,
    ILogger<UserInvitedConsumer> logger)
    : RabbitMqConsumerBase<UserInvitedEvent>(connectionFactory, scopeFactory, logger,
        MessagingConstants.Queues.UserInvited)
{
    protected override async Task HandleAsync(UserInvitedEvent message, Guid messageId, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
        var roleRepository = serviceProvider.GetRequiredService<IRoleRepository>();
        var passwordService = serviceProvider.GetRequiredService<IPasswordService>();
        var passwordHasherService = serviceProvider.GetRequiredService<IPasswordHasherService>();
        var inboxRepository = serviceProvider.GetRequiredService<IInboxRepository>();
        var outboxRepository = serviceProvider.GetRequiredService<IOutboxRepository>();
        var emailDeliveryRepository = serviceProvider.GetRequiredService<IEmailDeliveryRepository>();
        var mapper = serviceProvider.GetRequiredService<IMapper>();

        if (await inboxRepository.IsProcessedAsync(
                messageId,
                nameof(UserInvitedConsumer),
                cancellationToken))
        {
            return;
        }

        var exists = await userRepository.FindAsync(u => u.Email == message.Email, cancellationToken);
        if (exists is not null)
        {
            await inboxRepository.AddAsync(
                messageId,
                nameof(UserInvitedConsumer),
                cancellationToken);
            await userRepository.SaveAsync(cancellationToken);
            return;
        }

        var role = await roleRepository.GetAsync(r => r.Id == RoleIds.Client, cancellationToken);
        if (role is null)
        {
            throw new ConfigurationException(
                ConfigurationErrorType.MissingSeedData,
                string.Format(LogMessages.MissingSeedData, RoleIds.Client));
        }

        var entity = mapper.Map<User>(message);
        entity.Id = Guid.NewGuid();

        var userRole = new UserRole
        {
            UserId = entity.Id,
            RoleId = RoleIds.Client
        };
        entity.UserRoles = [userRole];

        var rndPassword = passwordService.GenerateRandomPassword();
        entity.PasswordHash = passwordHasherService.Hash(rndPassword);

        await userRepository.AddAsync(entity, cancellationToken);

        var @event = new UserCreatedEvent(
            entity.Id,
            entity.Email,
            entity.Name,
            entity.Surname,
            role.UserType,
            entity.AvatarPath,
            entity.OrganizationId);

        await outboxRepository.AddAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserCreated,
            @event,
            cancellationToken);

        await emailDeliveryRepository.AddConfirmationAsync(
            entity.Id,
            rndPassword,
            cancellationToken);

        await inboxRepository.AddAsync(
            messageId,
            nameof(UserInvitedConsumer),
            cancellationToken);

        await userRepository.SaveAsync(cancellationToken);
    }
}
