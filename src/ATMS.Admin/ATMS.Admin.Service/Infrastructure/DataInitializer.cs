using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Infrastructure.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Contracts.Events.Users;
using ATMS.Data.Constants;
using ATMS.Infrastructure.Options;
using ATMS.Messaging.Configuration;
using Microsoft.Extensions.Configuration;

namespace ATMS.Admin.Service.Infrastructure;

public sealed class DataInitializer(
    IConfiguration configuration,
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasherService passwordHasherService,
    IOutboxRepository outboxRepository) : IDataInitializer
{
    
    private readonly AdminOptions _adminOptions =
        configuration.GetSection(nameof(AdminOptions)).Get<AdminOptions>()
            ?? throw new ConfigurationException(ConfigurationErrorType.AdminSectionNotFound,
                string.Format(LogMessages.ConfigSectionNotFound, nameof(AdminOptions)));
    
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSuperAdminUserAsync(cancellationToken);
    }

    private async Task EnsureSuperAdminUserAsync(CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetAsync(r => r.Id == RoleIds.SuperAdmin, cancellationToken);

        if (role is null)
        {
            throw new ConfigurationException(
                ConfigurationErrorType.MissingSeedData,
                string.Format(LogMessages.MissingSeedData, RoleIds.SuperAdmin));
        }

        var user = await userRepository.FindAsync(
            u => u.NormalizedEmail == _adminOptions.Email.Trim().ToUpperInvariant(),
            cancellationToken);

        if (user is null)
        {
            var userId = Guid.NewGuid();
            user = new User
            {
                Id = userId,
                Email = _adminOptions.Email,
                Name = _adminOptions.Name,
                Surname = _adminOptions.Surname,
                NormalizedEmail = _adminOptions.Email.Trim().ToUpperInvariant(),
                PasswordHash = passwordHasherService.Hash(_adminOptions.Password),
                EmailConfirmed = true,
                IsAdmin = true,
                HasCompletedOnboarding = true,
                OnboardingCompletedAt = DateTime.UtcNow,
                LanguageId = DefaultValues.Language,
                AvatarPath = DefaultValues.UserAvatar,
                UserRoles = [new UserRole { RoleId = role.Id, UserId = userId }]
            };

            await userRepository.AddAsync(user, cancellationToken);
        }

        var @event = new UserCreatedEvent(
            user.Id,
            user.Email,
            user.Name,
            user.Surname,
            role.UserType,
            user.AvatarPath,
            user.OrganizationId,
            user.IsAdmin);

        var eventExists = await outboxRepository.ContainsAsync(
            MessagingConstants.Exchanges.UserEvents,
            MessagingConstants.RoutingKeys.UserCreated,
            @event,
            cancellationToken);

        if (!eventExists)
        {
            await outboxRepository.AddAsync(
                MessagingConstants.Exchanges.UserEvents,
                MessagingConstants.RoutingKeys.UserCreated,
                @event,
                cancellationToken);
        }

        await userRepository.SaveAsync(cancellationToken);
    }
}
