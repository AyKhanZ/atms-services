using ATMS.Admin.Service.Security;
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
            .ReturnsAsync([]);
 
        var result = await _accessTokenService.GenerateTokenAsync(user, CancellationToken.None);
 
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }
    
    [Fact]
    public async Task GenerateTokenAsync_ReturnsCorrectExpiration()
    {
        var user = CreateUser();
        UserRepositoryMock
            .Setup(r => r.GetRolesAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
 
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
            .ReturnsAsync([]);
 
        await _accessTokenService.GenerateTokenAsync(user, token);
 
        UserRepositoryMock.Verify(r => r.GetRolesAsync(user.Id, token), Times.Once);
    }
}
