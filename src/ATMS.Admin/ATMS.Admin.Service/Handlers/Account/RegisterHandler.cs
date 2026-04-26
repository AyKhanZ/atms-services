using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Email.Models;
using ATMS.Email.Services.Interfaces;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Contracts.Events.Users;
using ATMS.Infrastructure.Options;
using ATMS.Messaging.Configuration;
using ATMS.Messaging.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace ATMS.Admin.Service.Handlers.Account;

public class RegisterHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    ICurrentUser currentUser,
    IMapper mapper,
    IPasswordService passwordService,
    IPasswordHasherService passwordHasherService,
    IEmailConfirmationTokenService emailConfirmationTokenService,
    IEmailSender emailSender,
    IMessagePublisher messagePublisher,
    IConfiguration configuration)
    : IRequestHandler<RegisterCommand, UserModel>
{
    private readonly RedirectUrlOptions _redirectUrlOptions =
        configuration.GetSection(nameof(RedirectUrlOptions)).Get<RedirectUrlOptions>()
        ?? throw new ConfigurationException(ConfigurationErrorType.RedirectUrlSectionNotFound,
            string.Format(ExceptionMessages.ConfigSectionNotFound, nameof(RedirectUrlOptions)));

    public async Task<UserModel> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetAsync(r => r.Id == command.RoleId, cancellationToken);

        if (role is null)
        {
            throw new ConfigurationException(
                ConfigurationErrorType.MissingSeedData,
                string.Format(ExceptionMessages.MissingSeedData, command.RoleId));
        }
        
        var entity = mapper.Map<User>(command);
        entity.Id = Guid.NewGuid();

        var userRole = new UserRole
        {
            UserId = entity.Id,
            RoleId = role.Id
        };
        entity.UserRoles = [userRole];
        entity.InvitedById = currentUser.Id;
        entity.OrganizationId = command.OrganizationId;
        

        var rndPassword = passwordService.GenerateRandomPassword();
        entity.PasswordHash = passwordHasherService.Hash(rndPassword);

        await userRepository.CreateAsync(entity, cancellationToken);

        var emailConfirmationTokenResult = emailConfirmationTokenService.GenerateToken(entity);
        var link = GenerateConfirmationLink(emailConfirmationTokenResult.Token);

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

        return mapper.Map<UserModel>(entity);
    }

    private string GenerateConfirmationLink(string token) =>
        $"{_redirectUrlOptions.BaseUrl}/account/confirm?token={token}";
}