using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security;
using Moq;

namespace Admin.Services.Tests.Security;

public class ResetPasswordTokenServiceTest : BaseServiceTest
{
    private readonly ResetPasswordTokenService _resetPasswordTokenService;
    
    private const string FakeToken = "fake-reset-token";

    public ResetPasswordTokenServiceTest()
    {
        var passwordResetTokenRepositoryMock = new Mock<IPasswordResetTokenRepository>();
        _resetPasswordTokenService = new ResetPasswordTokenService(passwordResetTokenRepositoryMock.Object, UniqueTokenServiceMock.Object,
            BuildConfiguration());
        
        UniqueTokenServiceMock
            .Setup(s => s.GenerateUniqueAsync(It.IsAny<Func<string, Task<bool>>>(), It.IsAny<int>()))
            .ReturnsAsync(FakeToken);
 
        passwordResetTokenRepositoryMock
            .Setup(r => r.AddToListAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
    
    [Fact]
    public async Task GenerateTokenAsync_ReturnsGeneratedToken()
    {
        var user = CreateUser();
 
        var result = await _resetPasswordTokenService.GenerateTokenAsync(user, CancellationToken.None);
 
        Assert.Equal(FakeToken, result.Token);
    }
 
    [Fact]
    public async Task GenerateTokenAsync_ReturnsCorrectExpiration()
    {
        var user = CreateUser();
 
        var before = DateTime.UtcNow.AddHours(PasswordResetTokenExpirationInHours);
        var result = await _resetPasswordTokenService.GenerateTokenAsync(user, CancellationToken.None);
        var after = DateTime.UtcNow.AddHours(PasswordResetTokenExpirationInHours);
 
        Assert.InRange(result.ExpiresInHours, before, after);
    }
}
