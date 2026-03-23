using System.Security.Claims;
using ATMS.Admin.Service.Security;

namespace Admin.Services.Tests.Security;

public class EmailConfirmationTokenServiceTest : BaseServiceTest
{
    
    private readonly EmailConfirmationTokenService _emailConfirmationTokenService;
    
    public EmailConfirmationTokenServiceTest()
    {
        _emailConfirmationTokenService = new EmailConfirmationTokenService(BuildConfiguration());
    }
    
    [Fact]
    public void GenerateToken_ReturnsNonEmptyToken()
    {
        var user = CreateUser();
 
        var result = _emailConfirmationTokenService.GenerateToken(user);
 
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }
 
    [Fact]
    public void GenerateToken_ReturnsCorrectExpiration()
    {
        var user = CreateUser();
 
        var before = DateTime.UtcNow.AddHours(EmailConfirmationTokenExpirationInHours);
        var result = _emailConfirmationTokenService.GenerateToken(user);
        var after = DateTime.UtcNow.AddHours(EmailConfirmationTokenExpirationInHours);
 
        Assert.InRange(result.ExpiresInHours, before, after);
    }
 
    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ReturnsPrincipalWithCorrectEmail()
    {
        var user = CreateUser();
        var generated = _emailConfirmationTokenService.GenerateToken(user);
 
        var principal = await _emailConfirmationTokenService.ValidateTokenAsync(generated.Token);
 
        Assert.NotNull(principal);
        var email = principal.FindFirstValue(ClaimTypes.Email);
        Assert.Equal(user.Email, email);
    }
 
    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ReturnsNull()
    {
        var result = await _emailConfirmationTokenService.ValidateTokenAsync("invalid.token.here");
 
        Assert.Null(result);
    }
}
