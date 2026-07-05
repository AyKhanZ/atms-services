using ATMS.Admin.Data.Entities;
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

        var result = await _accessTokenService.GenerateTokenAsync(user, CancellationToken.None);

        var handler = new JsonWebTokenHandler();
        var jwt = handler.ReadJsonWebToken(result.Token);

        var claim = jwt.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.OrganizationId);

        Assert.Null(claim);
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
}
