using ATMS.Admin.Service.Security;
using Moq;

namespace Admin.Services.Tests.Security;

public class RefreshTokenServiceTest : BaseServiceTest
{
    
    private readonly RefreshTokenService _refreshTokenService;
    
    public RefreshTokenServiceTest()
    {
        _refreshTokenService = new RefreshTokenService(
            UserRepositoryMock.Object,
            UniqueTokenServiceMock.Object,
            BuildConfiguration());
    }
    
    [Fact]
    public async Task GenerateTokenAsync_ReturnsTokenFromUniqueTokenService()
    {
        var user = CreateUser();
        var token = Faker.Random.AlphaNumeric(100);
        UniqueTokenServiceMock
            .Setup(s => s.GenerateUniqueAsync(It.IsAny<Func<string, Task<bool>>>(),5))
            .ReturnsAsync(token);
 
        var result = await _refreshTokenService.GenerateTokenAsync(user, CancellationToken.None);
 
        Assert.Equal(token, result);
    }
    
    [Fact]
    public async Task GenerateTokenAsync_SetsCorrectRefreshTokenExpiration()
    {
        var user = CreateUser();
        var token = Faker.Random.AlphaNumeric(100);
        UniqueTokenServiceMock
            .Setup(s => s.GenerateUniqueAsync(It.IsAny<Func<string, Task<bool>>>(),5))
            .ReturnsAsync(token);
 
        var before = DateTime.UtcNow.AddDays(ValidRefreshExpirationInDays);
        await _refreshTokenService.GenerateTokenAsync(user, CancellationToken.None);
        var after = DateTime.UtcNow.AddDays(ValidRefreshExpirationInDays);
 
        Assert.InRange(user.RefreshTokenExpiresAt, before, after);
    }
    
    [Fact]
    public async Task GenerateTokenAsync_SetsRefreshTokenOnUser()
    {
        var user = CreateUser();
        var token = Faker.Random.AlphaNumeric(100);
        UniqueTokenServiceMock
            .Setup(s => s.GenerateUniqueAsync(It.IsAny<Func<string, Task<bool>>>(),5))
            .ReturnsAsync(token);
 
        await _refreshTokenService.GenerateTokenAsync(user, CancellationToken.None);
 
        Assert.Equal(token, user.RefreshToken);
    }
}
