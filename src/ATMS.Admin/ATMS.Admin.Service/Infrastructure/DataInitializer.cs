using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Infrastructure.Interfaces;
using ATMS.Admin.Service.Security.Interfaces;
using ATMS.Exceptions.Configuration;
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
                $"Configuration for section '{nameof(AdminOptions)}' is not found or could not be loaded.");
    
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
        var permissions = await permissionRepository.GetAsync(cancellationToken);

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = _adminOptions.RoleName,
            Description = "Super administrator with all permissions",
            RolePermissions = permissions.Select(p => new RolePermission
            {
                PermissionId = p.Id
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
        var role = await roleRepository.GetAsync(
            r => r.Name == _adminOptions.RoleName, cancellationToken);

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
