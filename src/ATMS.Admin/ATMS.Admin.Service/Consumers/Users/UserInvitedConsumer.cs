using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Contracts.Events.Users;
using ATMS.Data.Constants;
using ATMS.Email.Models;
using ATMS.Email.Services.Interfaces;
using ATMS.Infrastructure.Options;
using ATMS.Messaging.Configuration;
using ATMS.Messaging.Infrastructure;
using ATMS.Messaging.Interfaces;
using Microsoft.Extensions.Configuration;
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
    protected override async Task HandleAsync(UserInvitedEvent message, IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
        var roleRepository = serviceProvider.GetRequiredService<IRoleRepository>();
        var passwordService = serviceProvider.GetRequiredService<IPasswordService>();
        var passwordHasherService = serviceProvider.GetRequiredService<IPasswordHasherService>();
        var emailConfirmationTokenService = serviceProvider.GetRequiredService<IEmailConfirmationTokenService>();
        var emailSender = serviceProvider.GetRequiredService<IEmailSender>();
        var messagePublisher = serviceProvider.GetRequiredService<IMessagePublisher>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var exists = await userRepository.FindAsync(u => u.Email == message.Email, cancellationToken);
        if (exists is not null)
        {
            logger.LogError("User {MessageEmail} already exists", message.Email);
            return;
        }

        var redirectUrlOptions = configuration.GetSection(nameof(RedirectUrlOptions)).Get<RedirectUrlOptions>()
            ?? throw new ConfigurationException(
                ConfigurationErrorType.RedirectUrlSectionNotFound,
                string.Format(ExceptionMessages.ConfigSectionNotFound, nameof(RedirectUrlOptions)));

        var role = await roleRepository.GetAsync(r => r.Id == RoleIds.Client, cancellationToken);
        if (role is null)
        {
            throw new ConfigurationException(
                ConfigurationErrorType.MissingSeedData,
                string.Format(ExceptionMessages.MissingSeedData, RoleIds.Client));
        }

        var entity = new User
        {
            Id = Guid.NewGuid(),
            Email = message.Email,
            Name = message.Name,
            Surname = message.Surname,
            OrganizationId = message.OrganizationId,
            InvitedById = message.InvitedByUserId
        };

        var userRole = new UserRole
        {
            UserId = entity.Id,
            RoleId = RoleIds.Client
        };
        entity.UserRoles = [userRole];

        var rndPassword = passwordService.GenerateRandomPassword();
        entity.PasswordHash = passwordHasherService.Hash(rndPassword);

        await userRepository.CreateAsync(entity, cancellationToken);

        var emailConfirmationTokenResult = emailConfirmationTokenService.GenerateToken(entity);
        var link = $"{redirectUrlOptions.BaseUrl}/account/confirm?token={emailConfirmationTokenResult.Token}";

        await emailSender.SendAsync(entity.Email,
            new InviteModel
            {
                Email = entity.Email,
                Name = entity.Name,
                Surname = entity.Surname,
                Password = rndPassword,
                Link = link,
                DeadlineOfToken = emailConfirmationTokenResult.ExpiresInHours
            }, cancellationToken);

        var @event = new UserCreatedEvent(
            entity.Id,
            entity.Email,
            entity.Name,
            entity.Surname,
            role.UserType,
            entity.AvatarPath,
            entity.OrganizationId);

        await messagePublisher.PublishAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserCreated,
            @event,
            cancellationToken);
    }
}