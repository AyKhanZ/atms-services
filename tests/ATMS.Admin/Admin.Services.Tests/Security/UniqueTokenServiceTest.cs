using ATMS.Admin.Service.Security;
using ATMS.Application.Exceptions.Auth;
using ATMS.Application.Exceptions.Enums;

namespace Admin.Services.Tests.Security;

public class UniqueTokenServiceTest
{
    private readonly UniqueTokenService _uniqueTokenService = new();

    [Fact]
    public async Task GenerateUniqueAsync_WhenAllAttemptsExhausted_ThrowsAuthException()
    {
        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _uniqueTokenService.GenerateUniqueAsync(_ => Task.FromResult(true)));

        Assert.Equal(AuthErrorTypeEnum.TokenGenerationFailed, exception.AuthErrorType);
    }

    [Fact]
    public void Hash_ForSameToken_ReturnsSameValueThatIsNotTheToken()
    {
        const string token = "raw-token";

        var first = _uniqueTokenService.Hash(token);
        var second = _uniqueTokenService.Hash(token);

        Assert.Equal(first, second);
        Assert.NotEqual(token, first);
        Assert.True(first.Length <= 64);
    }

    [Fact]
    public void Hash_ForDifferentTokens_ReturnsDifferentValues()
    {
        Assert.NotEqual(_uniqueTokenService.Hash("token-a"), _uniqueTokenService.Hash("token-b"));
    }
}
