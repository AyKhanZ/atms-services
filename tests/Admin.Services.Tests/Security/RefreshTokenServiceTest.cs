using ATMS.Admin.Service.Security;
using Moq;

namespace Admin.Services.Tests.Security;

public class RefreshTokenServiceTest : BaseServiceTest
{
    private readonly RefreshTokenService _service;

    public RefreshTokenServiceTest()
    {
        _service = new RefreshTokenService(
            UserSessionRepositoryMock.Object,
            UniqueTokenServiceMock.Object,
            BuildConfiguration());
    }

    [Fact]
    public async Task GenerateTokenAsync_ReturnsHashedTokenWithoutPersistingRawToken()
    {
        const string token = "raw-refresh-token";
        UniqueTokenServiceMock
            .Setup(service => service.GenerateUniqueAsync(
                It.IsAny<Func<string, Task<bool>>>(),
                It.IsAny<int>()))
            .ReturnsAsync(token);

        var result = await _service.GenerateTokenAsync(null, CancellationToken.None);

        Assert.Equal(token, result.Token);
        Assert.NotEqual(token, result.TokenHash);
        Assert.Equal(_service.HashToken(token), result.TokenHash);
    }

    [Fact]
    public async Task GenerateTokenAsync_ForNewFamily_UsesConfiguredLifetimes()
    {
        UniqueTokenServiceMock
            .Setup(service => service.GenerateUniqueAsync(
                It.IsAny<Func<string, Task<bool>>>(),
                It.IsAny<int>()))
            .ReturnsAsync("token");
        var before = DateTime.UtcNow;

        var result = await _service.GenerateTokenAsync(null, CancellationToken.None);

        Assert.InRange(
            result.ExpiresAt,
            before.AddDays(ValidRefreshExpirationInDays),
            DateTime.UtcNow.AddDays(ValidRefreshExpirationInDays));
        Assert.InRange(
            result.FamilyExpiresAt,
            before.AddDays(MaxRefreshExpirationInDays),
            DateTime.UtcNow.AddDays(MaxRefreshExpirationInDays));
    }

    [Fact]
    public async Task GenerateTokenAsync_NeverExtendsPastFamilyExpiration()
    {
        UniqueTokenServiceMock
            .Setup(service => service.GenerateUniqueAsync(
                It.IsAny<Func<string, Task<bool>>>(),
                It.IsAny<int>()))
            .ReturnsAsync("token");
        var familyExpiration = DateTime.UtcNow.AddMinutes(5);

        var result = await _service.GenerateTokenAsync(
            familyExpiration,
            CancellationToken.None);

        Assert.Equal(familyExpiration, result.ExpiresAt);
        Assert.Equal(familyExpiration, result.FamilyExpiresAt);
    }
}
