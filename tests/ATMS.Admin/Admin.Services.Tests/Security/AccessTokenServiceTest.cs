using ATMS.Admin.Data.Entities;
using ATMS.Admin.Data.Entities.Dictionaries;
using ATMS.Admin.Service.Security;
using ATMS.Application.Constants;
using ATMS.Data.Constants;
using Microsoft.IdentityModel.JsonWebTokens;
using Moq;

namespace Admin.Services.Tests.Security;

public class AccessTokenServiceTest : BaseServiceTest
{
    private readonly AccessTokenService  _accessTokenService;

    public AccessTokenServiceTest()
    {
        _accessTokenService = new AccessTokenService(UserRepositoryMock.Object, BuildConfiguration());
    }
 
    [Fact]
    public async Task GenerateTokenAsync_ReturnsNonEmptyToken()
    {
        var user = CreateUser();

        UserRepositoryMock
            .Setup(r => r.GetRolesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateRoles());
        SetupPermissions(user.Id);

        var result = await _accessTokenService.GenerateTokenAsync(user, CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }
    
    [Fact]
    public async Task GenerateTokenAsync_ReturnsCorrectExpiration()
    {
        var user = CreateUser();

        UserRepositoryMock
            .Setup(r => r.GetRolesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateRoles());
        SetupPermissions(user.Id);

        var before = DateTime.UtcNow.AddMinutes(JwtValidExpirationMinutes);
        var result = await _accessTokenService.GenerateTokenAsync(user, CancellationToken.None);
        var after = DateTime.UtcNow.AddMinutes(JwtValidExpirationMinutes);

        Assert.InRange(result.ExpiresInMinutes, before, after);
    }
    
    [Fact]
    public async Task GenerateTokenAsync_PassesCancellationTokenToRepository()
    {
        var user = CreateUser();

        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        UserRepositoryMock
            .Setup(r => r.GetRolesAsync(user.Id, token))
            .ReturnsAsync(CreateRoles());
        SetupPermissions(user.Id, token);

        await _accessTokenService.GenerateTokenAsync(user, token);

        UserRepositoryMock.Verify(r => r.GetRolesAsync(user.Id, token), Times.Once);
    }
    
    [Fact]
    public async Task GenerateTokenAsync_AddsOrganizationIdClaim_When_NotEmployee()
    {
        // Arrange
        var user = CreateUser();
        user.OrganizationId = Guid.NewGuid();

        var roleId = Guid.NewGuid(); // not Employee

        UserRepositoryMock
            .Setup(r => r.GetRolesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateRoles(roleId, "Client"));
        SetupPermissions(user.Id);

        // Act
        var result = await _accessTokenService.GenerateTokenAsync(user, CancellationToken.None);

        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(result.Token);

        // Assert
        var claim = jwt.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.OrganizationId);

        Assert.NotNull(claim);
        Assert.Equal(user.OrganizationId.ToString(), claim!.Value);
    }
    
    [Fact]
    public async Task GenerateTokenAsync_DoesNotAddOrganizationIdClaim_ForEmployee()
    {
        var user = CreateUser();
        user.OrganizationId = Guid.NewGuid();

        UserRepositoryMock
            .Setup(r => r.GetRolesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateRoles(RoleIds.Employee, "Employee"));
        SetupPermissions(user.Id);

        var result = await _accessTokenService.GenerateTokenAsync(user, CancellationToken.None);

        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(result.Token);

        var claim = jwt.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.OrganizationId);

        Assert.Null(claim);
    }

    [Fact]
    public async Task GenerateTokenAsync_AddsPermissionClaims()
    {
        var user = CreateUser();
        var permissions = new[] { "UserView", "UserEdit" };

        UserRepositoryMock
            .Setup(r => r.GetRolesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateRoles());
        UserRepositoryMock
            .Setup(r => r.GetPermissionsAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions.Select(code => new Permission { Code = code }).ToList());

        var result = await _accessTokenService.GenerateTokenAsync(user, CancellationToken.None);

        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(result.Token);
        var permissionClaims = jwt.Claims
            .Where(claim => claim.Type == CustomClaimTypes.Permission)
            .Select(claim => claim.Value)
            .ToArray();

        Assert.Equal(permissions, permissionClaims);
    }

    [Fact]
    public async Task GenerateTokenAsync_Throws_WhenUserHasMoreThanOneRole()
    {
        var user = CreateUser();
        var roles = CreateRoles();
        roles.Add(new Role { Id = Guid.NewGuid(), Name = "Employee" });

        UserRepositoryMock
            .Setup(r => r.GetRolesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _accessTokenService.GenerateTokenAsync(user, CancellationToken.None));
    }
    
    private List<Role> CreateRoles(Guid? roleId = null, string? roleName = null)
    {
        return
        [
            new Role
            {
                Id = roleId ?? Guid.NewGuid(),
                Name = roleName ?? "Client"
            }
        ];
    }

    private void SetupPermissions(Guid userId, CancellationToken? cancellationToken = null)
    {
        UserRepositoryMock
            .Setup(r => r.GetPermissionsAsync(
                userId,
                cancellationToken ?? It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }
}
