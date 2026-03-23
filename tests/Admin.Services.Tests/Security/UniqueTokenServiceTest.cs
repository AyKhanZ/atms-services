using ATMS.Admin.Service.Exceptions.Auth;
using ATMS.Admin.Service.Security;

namespace Admin.Services.Tests.Security;

public class UniqueTokenServiceTest
{
    
    private readonly UniqueTokenService _uniqueTokenService = new();
    
    [Fact]
    public async Task GenerateUniqueAsync_WhenAllAttemptsExhausted_ThrowsAuthException()
    {
        var exception = await Assert.ThrowsAsync<AuthException>(() =>
            _uniqueTokenService.GenerateUniqueAsync(_ => Task.FromResult(true)));
 
        Assert.Equal(AuthErrorType.TokenGenerationFailed, exception.AuthErrorType);
    }
}
