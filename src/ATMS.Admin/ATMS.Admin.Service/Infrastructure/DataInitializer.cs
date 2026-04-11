using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Infrastructure.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Application.Exceptions.Configuration;
using ATMS.Application.Exceptions.Resources;
using ATMS.Infrastructure.Options;
using Microsoft.Extensions.Configuration;

namespace ATMS.Admin.Service.Infrastructure;

public sealed class DataInitializer(
    IConfiguration configuration,
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPermissionRepository permissionRepository,
    IPasswordHasherService passwordHasherService) : IDataInitializer
{
    
    private readonly AdminOptions _adminOptions =
        configuration.GetSection(nameof(AdminOptions)).Get<AdminOptions>()
            ?? throw new ConfigurationException(ConfigurationErrorType.AdminSectionNotFound,
                string.Format(ExceptionMessages.ConfigSectionNotFound, nameof(AdminOptions)));
    
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureSuperAdminRoleAsync(cancellationToken);
        await EnsureSuperAdminUserAsync(cancellationToken);
    }

    private async Task EnsureSuperAdminRoleAsync(CancellationToken cancellationToken)
    {
        var roleExists = await roleRepository.IsExistAsync(
            r => r.Name == _adminOptions.RoleName, cancellationToken);

        if (roleExists)
        {
            return;
        }
        var permissionIds = await permissionRepository.GetIdsAsync(cancellationToken);

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = _adminOptions.RoleName,
            Description = "Super administrator with all permissions",
            RolePermissions = permissionIds.Select(id => new RolePermission
            {
                PermissionId = id
            }).ToList()
        };

        await roleRepository.CreateAsync(role, cancellationToken);
    }

    private async Task EnsureSuperAdminUserAsync(CancellationToken cancellationToken)
    {
        var userExists = await userRepository.IsExistAsync(
            u => u.Email == _adminOptions.Email, cancellationToken);

        if (userExists)
        {
            return;
        }
        var role = await roleRepository.GetAsync(r => r.Name == _adminOptions.RoleName, cancellationToken);

        var userId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            Email = _adminOptions.Email,
            Name = _adminOptions.Name,
            Surname = _adminOptions.Surname,
            PasswordHash = passwordHasherService.Hash(_adminOptions.Password),
            EmailConfirmed = true,
            HasCompletedSurvey = true,
            UserRoles = [new UserRole { RoleId = role!.Id, UserId = userId }]
        };

        await userRepository.CreateAsync(user, cancellationToken);
    }
}
