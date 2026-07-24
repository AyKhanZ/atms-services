using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Application.Interfaces;
using ATMS.Contracts.Events.Users;
using ATMS.Messaging.Configuration;
using AutoMapper;
using MediatR;

namespace ATMS.Admin.Service.Handlers.Account;

public class RegisterHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    ICurrentUser currentUser,
    IMapper mapper,
    IPasswordService passwordService,
    IPasswordHasherService passwordHasherService,
    IOutboxRepository outboxRepository,
    IEmailDeliveryRepository emailDeliveryRepository)
    : IRequestHandler<RegisterCommand, UserModel>
{
    public async Task<UserModel> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetAsync(r => r.Id == command.RoleId, cancellationToken);

        if (role is null)
        {
            throw new ConfigurationException(
                ConfigurationErrorType.MissingSeedData,
                string.Format(LogMessages.MissingSeedData, command.RoleId));
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

        await userRepository.SaveAsync(cancellationToken);

        return mapper.Map<UserModel>(entity);
    }
}
