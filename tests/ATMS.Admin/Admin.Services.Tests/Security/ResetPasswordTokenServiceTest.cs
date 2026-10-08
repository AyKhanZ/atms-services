using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using ATMS.Infrastructure.Options;
using ATMS.Admin.Data.Entities.Tokens;
using ATMS.Admin.Data.Repositories.Interfaces;
using ATMS.Admin.Service.Security;
using Moq;

namespace Admin.Services.Tests.Security;

public class ResetPasswordTokenServiceTest : BaseServiceTest
{
    private readonly ResetPasswordTokenService _resetPasswordTokenService;
    private readonly Mock<IPasswordResetTokenRepository> _passwordResetTokenRepositoryMock = new();

    private const string FakeToken = "fake-reset-token";
    private const string FakeTokenHash = "fake-reset-token-hash";

    public ResetPasswordTokenServiceTest()
    {
        _resetPasswordTokenService = new ResetPasswordTokenService(_passwordResetTokenRepositoryMock.Object, UniqueTokenServiceMock.Object,
            Options.Create(BuildConfiguration().GetSection(nameof(JwtOptions)).Get<JwtOptions>()!));

        UniqueTokenServiceMock
            .Setup(s => s.GenerateUniqueAsync(It.IsAny<Func<string, Task<bool>>>(), It.IsAny<int>()))
            .ReturnsAsync(FakeToken);
        UniqueTokenServiceMock
            .Setup(s => s.Hash(FakeToken))
            .Returns(FakeTokenHash);

        _passwordResetTokenRepositoryMock
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
    public async Task GenerateTokenAsync_StoresOnlyTheHash()
    {
        var user = CreateUser();

        await _resetPasswordTokenService.GenerateTokenAsync(user, CancellationToken.None);

        _passwordResetTokenRepositoryMock.Verify(r => r.AddToListAsync(
                It.Is<PasswordResetToken>(token => token.TokenHash == FakeTokenHash && token.UserId == user.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
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
