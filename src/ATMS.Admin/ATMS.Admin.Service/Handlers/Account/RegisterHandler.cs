using ATMS.Admin.Contracts.Commands.Account;
using ATMS.Admin.Contracts.Enums;
using ATMS.Admin.Contracts.Models.Users;
using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Email.Models;
using ATMS.Email.Services.Interfaces;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Data.Constants;
using ATMS.Infrastructure.Options;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace ATMS.Admin.Service.Handlers.Account;

public class RegisterHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IMapper mapper,
    IPasswordService passwordService,
    IPasswordHasherService passwordHasherService,
    IEmailConfirmationTokenService emailConfirmationTokenService,
    IEmailSender emailSender,
    IConfiguration configuration)
    : IRequestHandler<RegisterCommand, UserModel>
{
    private readonly RedirectUrlOptions _redirectUrlOptions =
        configuration.GetSection(nameof(RedirectUrlOptions)).Get<RedirectUrlOptions>()
        ?? throw new ConfigurationException(ConfigurationErrorType.RedirectUrlSectionNotFound,
            string.Format(ExceptionMessages.ConfigSectionNotFound, nameof(RedirectUrlOptions)));

    public async Task<UserModel> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var role = await ResolveRoleAsync(command.UserTypeId, cancellationToken);

        var entity = mapper.Map<User>(command);
        entity.Id = Guid.NewGuid();

        var userRole = new UserRole
        {
            UserId = entity.Id,
            RoleId = role.Id
        };
        entity.UserRoles = [userRole];

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

        return mapper.Map<UserModel>(entity);
    }

    private async Task<Role> ResolveRoleAsync(UserTypeEnum userType, CancellationToken cancellationToken)
    {
        var roleId = userType switch
        {
            UserTypeEnum.Agent => RoleIds.Agent,
            UserTypeEnum.Client => RoleIds.Client,
            UserTypeEnum.ClientManager => RoleIds.ClientManager,
            _ => throw new ConfigurationException(
                ConfigurationErrorType.MissingSeedData,
                $"Unsupported user type: {userType}")
        };

        var role = await roleRepository.GetAsync(r => r.Id == roleId, cancellationToken);

        if (role is null)
        {
            throw new ConfigurationException(
                ConfigurationErrorType.MissingSeedData,
                string.Format(ExceptionMessages.MissingSeedData, roleId));
        }

        return role;
    }

    private string GenerateConfirmationLink(string token) =>
        $"{_redirectUrlOptions.BaseUrl}/account/confirm?token={token}";
}